using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Models.Importing;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class ImportStatementViewModel : ViewModelBase
{
    private static readonly string HelpDirectory =
        Path.Combine(AppContext.BaseDirectory, "Models", "Importing", "Help");

    private readonly DatabaseService _db;
    private readonly DialogService   _dialogService;

    private Dictionary<string, string> _accountImporterTypes = new();
    private IImporter? _currentImporter;

    public ImportStatementViewModel(DatabaseService db, DialogService dialogService)
    {
        _db            = db;
        _dialogService = dialogService;
    }

    public ObservableCollection<string> AccountNames { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    private string? _selectedAccountName;

    [ObservableProperty]
    private string _selectedImporterTypeName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    private string? _transactionFilePath;

    [ObservableProperty]
    private string _helpText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectTransactionFileCommand))]
    private bool _isImporting;

    partial void OnSelectedAccountNameChanged(string? value) => _ = UpdateImporterAsync(value);

    private async Task UpdateImporterAsync(string? accountName)
    {
        _currentImporter = null;
        StatusMessage     = string.Empty;

        var importerTypeText = accountName is not null && _accountImporterTypes.TryGetValue(accountName, out var t)
            ? t
            : string.Empty;

        SelectedImporterTypeName = string.IsNullOrEmpty(importerTypeText) ? "(No importer assigned)" : importerTypeText;

        if (string.IsNullOrEmpty(importerTypeText) || !Enum.TryParse<ImporterType>(importerTypeText, out var importerType))
        {
            HelpText = string.Empty;
            return;
        }

        _currentImporter = ImporterFactory.GetImporter(importerType);
        HelpText = await LoadHelpTextAsync(_currentImporter.HelpFileName);
    }

    private static async Task<string> LoadHelpTextAsync(string helpFileName)
    {
        var path = Path.Combine(HelpDirectory, helpFileName);
        return File.Exists(path)
            ? await File.ReadAllTextAsync(path)
            : $"No help file found for '{helpFileName}'.";
    }

    public override async Task RefreshAsync()
    {
        if (!_db.IsOpen) return;

        var accounts = (await _db.QueryAsync<Account>("Accounts/GetAll.sql")).ToList();
        _accountImporterTypes = accounts.ToDictionary(a => a.Name, a => a.ImporterType);

        AccountNames.Clear();
        foreach (var a in accounts)
            AccountNames.Add(a.Name);
    }

    private bool CanSelectTransactionFile() => !IsImporting;

    [RelayCommand(CanExecute = nameof(CanSelectTransactionFile))]
    private async Task SelectTransactionFile()
    {
        var path = await _dialogService.PickTransactionFileAsync();
        if (string.IsNullOrEmpty(path)) return;

        TransactionFilePath = path;
    }

    private bool CanImport() =>
        !IsImporting &&
        _currentImporter is not null &&
        !string.IsNullOrWhiteSpace(SelectedAccountName) &&
        !string.IsNullOrWhiteSpace(TransactionFilePath);

    [RelayCommand(CanExecute = nameof(CanImport))]
    private async Task Import()
    {
        if (_currentImporter is null || SelectedAccountName is null || TransactionFilePath is null) return;

        IsImporting   = true;
        StatusMessage = "Importing…";

        try
        {
            var transactions = await _currentImporter.ImportAsync(TransactionFilePath);

            var imported = 0;
            foreach (var t in transactions)
            {
                t.AccountName = SelectedAccountName;
                if (string.IsNullOrEmpty(t.Id))
                    t.Id = $"{t.Date:yyyy-MM-dd}|{t.Name}|{t.Amount}";

                var rowsAffected = await _db.ExecuteQueryAsync("Transactions/Insert.sql", new
                {
                    t.Id,
                    Date                 = t.Date.ToString("yyyy-MM-dd"),
                    t.Name,
                    t.Description,
                    t.Amount,
                    t.BudgetCategoryName,
                    t.UserAdjustedCategory,
                    t.AccountName
                });

                if (rowsAffected > 0) imported++;
            }

            var skipped = transactions.Count - imported;
            StatusMessage = skipped > 0
                ? $"Imported {imported} of {transactions.Count} transaction(s) — {skipped} duplicate(s) skipped."
                : $"Imported {imported} transaction(s).";
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        SelectedAccountName      = null;
        SelectedImporterTypeName = string.Empty;
        TransactionFilePath      = null;
        HelpText                 = string.Empty;
        StatusMessage            = string.Empty;
        _currentImporter         = null;
    }
}