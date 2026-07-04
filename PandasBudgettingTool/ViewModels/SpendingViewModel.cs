using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class SpendingViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public SpendingViewModel(DatabaseService db) => _db = db;

    [ObservableProperty]
    private DateTime? _fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime? _toDate = new DateTime(
        DateTime.Today.Year,
        DateTime.Today.Month,
        DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

    public ObservableCollection<SpendingCategoryNodeViewModel> RootNodes { get; } = [];

    /// <summary>Sum of SpendingForTerm across every category (not just roots) where IsExcludedFromSpendingTotal is false
    /// and spending for term < 0 .</summary>
    [ObservableProperty]
    private decimal _netCashFlow;

    /// <summary>Sum of each category's own BudgetTarget where IsExcludedFromSpendingTotal is false. and spending for term < 0 </summary>
    [ObservableProperty]
    private decimal _totalCashFlow;
    
    /// <summary>Sum of SpendingForTerm across every category (not just roots) where IsExcludedFromSpendingTotal is false.</summary>
    [ObservableProperty]
    private decimal _netExpenses;

    /// <summary>Sum of each category's own BudgetTarget where IsExcludedFromSpendingTotal is false.</summary>
    [ObservableProperty]
    private decimal _totalExpenses;

    /// <summary>Raised when a category row's Transactions button is clicked — args are the category name and the current date range.</summary>
    public event Action<string, DateTime, DateTime>? OpenTransactionsForCategoryRequested;

    public void RequestOpenTransactionsForCategory(string categoryName)
    {
        var fromDate = FromDate ?? DateTime.Today.AddMonths(-6);
        var toDate   = ToDate   ?? DateTime.Today;
        OpenTransactionsForCategoryRequested?.Invoke(categoryName, fromDate, toDate);
    }

    public override async Task RefreshAsync()
    {
        RootNodes.Clear();
        if (!_db.IsOpen) return;

        var categories = (await _db.QueryAsync<BudgetCategory>("BudgetCategories/GetAll.sql")).ToList();

        var nodesByName = categories.ToDictionary(c => c.Name, c => new SpendingCategoryNodeViewModel(c));

        foreach (var c in categories)
        {
            if (c.Parent is not null && nodesByName.TryGetValue(c.Parent, out var parentNode))
                parentNode.Children.Add(nodesByName[c.Name]);
        }

        foreach (var c in categories.Where(c => c.Parent is null))
            RootNodes.Add(nodesByName[c.Name]);

        var fromDate = FromDate ?? DateTime.Today.AddMonths(-6);
        var toDate   = ToDate   ?? DateTime.Today;

        var spendRows = await _db.QueryRawAsync<CategorySpend>(
            "SELECT BudgetCategoryName, SUM(Amount) AS Total FROM [Transaction] " +
            "WHERE Date >= @FromDate AND Date <= @ToDate AND BudgetCategoryName IS NOT NULL " +
            "GROUP BY BudgetCategoryName",
            new { FromDate = fromDate.ToString("yyyy-MM-dd"), ToDate = toDate.ToString("yyyy-MM-dd") });

        // Amounts are negative = expense; 
        var directSpend = spendRows.ToDictionary(r =>
            r.BudgetCategoryName, r => r.Total);

        foreach (var root in RootNodes)
            ComputeRollup(root, directSpend);

        // Summed per-category (not the tree's rolled-up totals) so parent/child amounts aren't double-counted.
        var nonExcludedCategories = categories.Where(c => !c.IsExcludedFromSpendingTotal).ToList();
        var nonExcludeExpenseCats = nonExcludedCategories.Where(c => c.BudgetTarget < 0).ToList();
        NetCashFlow = nonExcludedCategories.Sum(c => directSpend.GetValueOrDefault(c.Name));
        TotalCashFlow = nonExcludedCategories.Sum(c => c.BudgetTarget ?? 0);
        NetExpenses = nonExcludeExpenseCats.Sum(c => directSpend.GetValueOrDefault(c.Name));
        TotalExpenses = nonExcludeExpenseCats.Sum(c => c.BudgetTarget ?? 0);
    }

    /// <summary>No editable fields on this page — just reloads and recomputes for the current date range.</summary>
    [RelayCommand]
    private async Task ApplyFilter() => await RefreshAsync();

    /// <summary>Recursively rolls up budget targets and actual spend; returns this node's own (budget, spend) totals for its parent.</summary>
    private static (decimal budgetTotal, decimal spendTotal) ComputeRollup(
        SpendingCategoryNodeViewModel node,
        IReadOnlyDictionary<string, decimal> directSpendByCategory)
    {
        var budgetSum = node.OwnBudgetTarget;
        var spendSum  = directSpendByCategory.GetValueOrDefault(node.Name);

        foreach (var child in node.Children)
        {
            var (childBudget, childSpend) = ComputeRollup(child, directSpendByCategory);
            budgetSum += childBudget;
            spendSum  += childSpend;
        }

        node.BudgetTarget    = budgetSum;
        node.SpendingForTerm = spendSum;
        node.Remaining       = spendSum - budgetSum;

        return (budgetSum, spendSum);
    }

    private class CategorySpend
    {
        public string BudgetCategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}