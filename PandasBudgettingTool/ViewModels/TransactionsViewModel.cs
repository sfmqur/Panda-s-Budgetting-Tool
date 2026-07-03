using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class TransactionsViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public TransactionsViewModel(DatabaseService db) => _db = db;

    // ── Filter properties ─────────────────────────────────────────────────────

    [ObservableProperty]
    private DateTime? _fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime? _toDate = new DateTime(
        DateTime.Today.Year,
        DateTime.Today.Month,
        DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

    public ObservableCollection<AccountFilterItem> AccountFilters { get; } = [];

    // Shared dropdown option lists — row VMs hold references to these
    public ObservableCollection<string?> BudgetCategoryOptions { get; } = [null];
    public ObservableCollection<string?> AccountOptions        { get; } = [null];

    // ── Data ──────────────────────────────────────────────────────────────────

    public ObservableCollection<TransactionRowViewModel> Transactions { get; } = [];

    // ── Public API ────────────────────────────────────────────────────────────

    public override async Task RefreshAsync()
    {
        await LoadAccountFiltersAsync();
        await LoadDropdownOptionsAsync();
        await LoadTransactionsAsync();
    }

    public override async Task SaveAsync()
    {
        if (!_db.IsOpen) return;

        foreach (var row in Transactions)
            await _db.ExecuteQueryAsync("Transactions/Update.sql", row.ToUpdateParam());
    }

    [RelayCommand]
    private async Task ApplyFilter() => await LoadTransactionsAsync();

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task LoadDropdownOptionsAsync()
    {
        if (!_db.IsOpen) return;

        BudgetCategoryOptions.Clear();
        BudgetCategoryOptions.Add(null);
        foreach (var n in await _db.QueryAsync<string>("BudgetCategories/GetAllNames.sql"))
            BudgetCategoryOptions.Add(n);

        AccountOptions.Clear();
        AccountOptions.Add(null);
        foreach (var n in await _db.QueryAsync<string>("Accounts/GetAllNames.sql"))
            AccountOptions.Add(n);
    }

    private async Task LoadAccountFiltersAsync()
    {
        if (!_db.IsOpen) return;

        // Remember which accounts the user had selected before refreshing
        var prevSelected = AccountFilters.Count > 0
            ? AccountFilters.Where(a => a.IsSelected).Select(a => a.Name).ToHashSet()
            : null; // null = first load → select all

        AccountFilters.Clear();

        // "(No Account)" entry — matches transactions with NULL AccountName
        AccountFilters.Add(new AccountFilterItem
        {
            IsNoAccount = true,
            IsSelected  = prevSelected is null || prevSelected.Contains(string.Empty)
        });

        var names = await _db.QueryAsync<string>("Accounts/GetAllNames.sql");
        foreach (var name in names)
        {
            AccountFilters.Add(new AccountFilterItem
            {
                Name       = name,
                IsSelected = prevSelected is null || prevSelected.Contains(name)
            });
        }
    }

    private async Task LoadTransactionsAsync()
    {
        if (!_db.IsOpen) return;

        var fromDate = (FromDate ?? DateTime.Today).ToString("yyyy-MM-dd");
        var toDate   = (ToDate   ?? DateTime.Today).ToString("yyyy-MM-dd");

        var selectedAccounts = AccountFilters
            .Where(a => !a.IsNoAccount && a.IsSelected)
            .Select(a => a.Name)
            .ToList();

        var includeNoAccount = AccountFilters
            .FirstOrDefault(a => a.IsNoAccount)?.IsSelected ?? true;

        // Build WHERE clause dynamically because IN lists can't be static SQL
        var sql = BuildTransactionQuery(selectedAccounts, includeNoAccount);

        var param = new
        {
            FromDate     = fromDate,
            ToDate       = toDate,
            AccountNames = selectedAccounts
        };

        var rows = await _db.QueryRawAsync<Transaction>(sql, param);

        Transactions.Clear();
        foreach (var t in rows)
            Transactions.Add(new TransactionRowViewModel(t, BudgetCategoryOptions, AccountOptions));
    }

    private static string BuildTransactionQuery(
        IReadOnlyCollection<string> selectedAccounts,
        bool includeNoAccount)
    {
        var sb = new StringBuilder(
            "SELECT * FROM [Transaction] WHERE Date >= @FromDate AND Date <= @ToDate");

        var acctClauses = new List<string>();
        if (selectedAccounts.Count > 0) acctClauses.Add("AccountName IN @AccountNames");
        if (includeNoAccount)           acctClauses.Add("AccountName IS NULL");

        if (acctClauses.Count > 0)
            sb.Append($" AND ({string.Join(" OR ", acctClauses)})");

        sb.Append(" ORDER BY Date DESC, Name");
        return sb.ToString();
    }
}