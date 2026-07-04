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

public partial class BudgetViewModel : ViewModelBase
{
    private readonly DatabaseService _db;
    private List<BudgetCategoryNodeViewModel> _allNodes = [];

    public BudgetViewModel(DatabaseService db) => _db = db;

    [ObservableProperty]
    private DateTime? _fromDate = DateTime.Today.AddMonths(-6);

    [ObservableProperty]
    private DateTime? _toDate = DateTime.Today;

    public ObservableCollection<BudgetCategoryNodeViewModel> RootNodes { get; } = [];

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

        var nodesByName = categories.ToDictionary(c => c.Name, c => new BudgetCategoryNodeViewModel(c));
        _allNodes = nodesByName.Values.ToList();

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

        // Amounts are negative = expense; flip sign so "spend" reads as a positive figure.
        var directSpend = spendRows.ToDictionary(r => r.BudgetCategoryName, r => -r.Total);

        var daysInRange = Math.Max(1.0, (toDate - fromDate).TotalDays);

        foreach (var root in RootNodes)
            ComputeRollup(root, directSpend, daysInRange);
    }

    public override async Task SaveAsync()
    {
        if (!_db.IsOpen) return;

        foreach (var node in _allNodes)
            await _db.ExecuteQueryAsync("BudgetCategories/UpdateBudgetTarget.sql", node.ToUpdateParam());
    }

    /// <summary>Saves current budget targets, then reloads and recomputes every calculated field.</summary>
    [RelayCommand]
    private async Task ApplyFilter()
    {
        await SaveAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task Save() => await SaveAsync();

    // todo 30 day avg  math seems funky, Something isn't working here, especially with negative numbers. 
    /// <summary>Recursively rolls up budget targets and actual spend; returns this node's own (target, spend) totals for its parent.</summary>
    private static (decimal budgetTotal, decimal spendTotal) ComputeRollup(
        BudgetCategoryNodeViewModel node,
        IReadOnlyDictionary<string, decimal> directSpendByCategory,
        double daysInRange)
    {
        var childrenBudgetSum = 0m;
        var spendRollup = directSpendByCategory.GetValueOrDefault(node.Name);

        foreach (var child in node.Children)
        {
            var (childBudgetTotal, childSpendTotal) = ComputeRollup(child, directSpendByCategory, daysInRange);
            childrenBudgetSum += childBudgetTotal;
            spendRollup       += childSpendTotal;
        }

        node.ChildrenBudgetSum = childrenBudgetSum;
        node.TotalBudget       = (node.OwnBudgetTarget ?? 0) + childrenBudgetSum;
        node.AverageSpend      = -1* spendRollup / (decimal)(daysInRange / 30.0);

        return (node.TotalBudget, spendRollup);
    }

    private class CategorySpend
    {
        public string BudgetCategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}