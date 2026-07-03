using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class CreateAccountViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public CreateAccountViewModel(DatabaseService db) => _db = db;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isMinusSignAnExpense = true;

    [ObservableProperty]
    private string _importerType = string.Empty;

    // Empty string = no importer assigned
    public IReadOnlyList<string> ImporterTypes { get; } =
        [string.Empty, .. Enum.GetNames<Importers>()];

    private bool CanCreate() => !string.IsNullOrWhiteSpace(Name);

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (!_db.IsOpen) return;

        await _db.ExecuteQueryAsync("Accounts/Insert.sql", new
        {
            Name,
            IsMinusSignAnExpense,
            ImporterType
        });

        Clear();
    }

    [RelayCommand]
    private void Clear()
    {
        Name = string.Empty;
        IsMinusSignAnExpense = true;
        ImporterType = string.Empty;
    }

    public override async Task SaveAsync()
    {
        if (CanCreate()) await Create();
    }
}