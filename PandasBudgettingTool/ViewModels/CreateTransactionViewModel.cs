using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class CreateTransactionViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public CreateTransactionViewModel(DatabaseService db) => _db = db;

    public ObservableCollection<string> AccountNames        { get; } = [];
    public ObservableCollection<string> BudgetCategoryNames { get; } = [];

    [ObservableProperty]
    private DateTime? _date = DateTime.Today;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private decimal? _amount;

    [ObservableProperty]
    private string? _selectedBudgetCategoryName;

    [ObservableProperty]
    private bool _userAdjustedCategory;

    [ObservableProperty]
    private string? _selectedAccountName;

    public async Task LoadOptionsAsync()
    {
        if (!_db.IsOpen) return;

        AccountNames.Clear();
        foreach (var n in await _db.QueryAsync<string>("Accounts/GetAllNames.sql"))
            AccountNames.Add(n);

        BudgetCategoryNames.Clear();
        foreach (var n in await _db.QueryAsync<string>("BudgetCategories/GetAllNames.sql"))
            BudgetCategoryNames.Add(n);
    }

    private bool CanCreate() =>
        !string.IsNullOrWhiteSpace(Name) && Amount.HasValue;

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (!_db.IsOpen) return;

        var date   = (Date ?? DateTime.Today).ToString("yyyy-MM-dd");
        var amount = Amount ?? 0m;
        var id     = $"{date}|{Name}|{amount}";

        await _db.ExecuteQueryAsync("Transactions/Insert.sql", new
        {
            Id                   = id,
            Date                 = date,
            Name,
            Description,
            Amount               = amount,
            BudgetCategoryName   = string.IsNullOrEmpty(SelectedBudgetCategoryName) ? null : SelectedBudgetCategoryName,
            UserAdjustedCategory,
            AccountName          = string.IsNullOrEmpty(SelectedAccountName) ? null : SelectedAccountName
        });

        Clear();
    }

    [RelayCommand]
    private void Clear()
    {
        Date                     = DateTime.Today;
        Name                     = string.Empty;
        Description              = string.Empty;
        Amount                   = null;
        SelectedBudgetCategoryName = null;
        UserAdjustedCategory     = false;
        SelectedAccountName      = null;
    }
}