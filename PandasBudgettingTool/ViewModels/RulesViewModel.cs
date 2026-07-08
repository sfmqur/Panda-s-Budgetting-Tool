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

public partial class RulesViewModel : ViewModelBase
{
    public const string AllBudgetCategoriesFilter = "(All)";
    private const string NoneBudgetCategoryFilter  = "(None)";

    private readonly DatabaseService _db;
    private readonly DialogService   _dialogService;
    private readonly RuleEngine      _ruleEngine;
    private List<RuleTreeNodeViewModel> _allRuleNodes  = [];
    private List<RuleCategory>          _allCategories = [];

    public RulesViewModel(DatabaseService db, DialogService dialogService, RuleEngine ruleEngine)
    {
        _db            = db;
        _dialogService = dialogService;
        _ruleEngine    = ruleEngine;
    }

    [ObservableProperty]
    private DateTime? _fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime? _toDate = new DateTime(
        DateTime.Today.Year,
        DateTime.Today.Month,
        DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

    public ObservableCollection<string> BudgetCategoryFilterOptions { get; } = [AllBudgetCategoriesFilter];

    [ObservableProperty]
    private string _selectedBudgetCategoryFilter = AllBudgetCategoriesFilter;

    partial void OnSelectedBudgetCategoryFilterChanged(string value) => BuildTree();

    public ObservableCollection<RuleTreeNode> RootNodes { get; } = [];

    /// <summary>Raised when a Rule row is double-clicked — the argument is the Rule's Name.</summary>
    public event Action<string>? OpenRuleConditionsRequested;

    public void RequestOpenRuleConditions(string ruleName) => OpenRuleConditionsRequested?.Invoke(ruleName);

    public override async Task RefreshAsync()
    {
        RootNodes.Clear();
        if (!_db.IsOpen) return;

        _allCategories = (await _db.QueryAsync<RuleCategory>("RuleCategories/GetAll.sql")).ToList();
        var rules      = (await _db.QueryAsync<Rule>("Rules/GetAll.sql")).ToList();

        var budgetCategoryNames = (await _db.QueryAsync<string>("BudgetCategories/GetAllNames.sql")).ToList();

        var budgetCategoryOptions = new List<string?> { null };
        budgetCategoryOptions.AddRange(budgetCategoryNames);

        var ruleCategoryOptions = new List<string?> { null };
        ruleCategoryOptions.AddRange(_allCategories.Select(c => c.Name));

        _allRuleNodes = rules
            .Select(r => new RuleTreeNodeViewModel(r, budgetCategoryOptions, ruleCategoryOptions))
            .ToList();

        // Remember the previously selected filter so refreshing doesn't reset it.
        var prevFilter = SelectedBudgetCategoryFilter;

        BudgetCategoryFilterOptions.Clear();
        BudgetCategoryFilterOptions.Add(AllBudgetCategoriesFilter);
        BudgetCategoryFilterOptions.Add(NoneBudgetCategoryFilter);
        foreach (var n in budgetCategoryNames)
            BudgetCategoryFilterOptions.Add(n);

        var newFilter = BudgetCategoryFilterOptions.Contains(prevFilter) ? prevFilter : AllBudgetCategoriesFilter;
        if (newFilter == SelectedBudgetCategoryFilter)
            BuildTree(); // setter won't raise OnChanged if unchanged, so build explicitly
        else
            SelectedBudgetCategoryFilter = newFilter; // OnSelectedBudgetCategoryFilterChanged will call BuildTree()
    }

    /// <summary>Rebuilds the visible tree from the loaded Rules/RuleCategories, applying the current Budget Category filter.
    /// All Rule Categories always stay visible; only leaf Rule nodes are filtered.</summary>
    private void BuildTree()
    {
        RootNodes.Clear();

        var categoryNodes = _allCategories.ToDictionary(
            c => c.Name,
            c => new RuleCategoryTreeNodeViewModel(c.Name));

        // Sub-categories first …
        foreach (var cat in _allCategories)
        {
            if (cat.ParentRuleCategoryName is not null
                && categoryNodes.TryGetValue(cat.ParentRuleCategoryName, out var parentNode))
            {
                parentNode.Children.Add(categoryNodes[cat.Name]);
            }
        }

        var filter = SelectedBudgetCategoryFilter;
        var visibleRuleNodes = _allRuleNodes.Where(r => MatchesBudgetCategoryFilter(r, filter)).ToList();

        // … then the rules that belong directly to each category.
        foreach (var ruleNode in visibleRuleNodes)
        {
            if (ruleNode.RuleCategoryName is not null
                && categoryNodes.TryGetValue(ruleNode.RuleCategoryName, out var parentNode))
            {
                parentNode.Children.Add(ruleNode);
            }
        }

        foreach (var cat in _allCategories.Where(c => c.ParentRuleCategoryName is null))
            RootNodes.Add(categoryNodes[cat.Name]);

        foreach (var ruleNode in visibleRuleNodes.Where(r => r.RuleCategoryName is null))
            RootNodes.Add(ruleNode);
    }

    private static bool MatchesBudgetCategoryFilter(RuleTreeNodeViewModel rule, string filter) => filter switch
    {
        AllBudgetCategoriesFilter  => true,
        NoneBudgetCategoryFilter   => rule.BudgetCategoryName is null,
        _                          => rule.BudgetCategoryName == filter
    };

    public override async Task SaveAsync()
    {
        if (!_db.IsOpen) return;

        foreach (var row in _allRuleNodes)
        {
            if (!string.IsNullOrWhiteSpace(row.Name) && row.Name != row.OriginalName)
            {
                await _db.ExecuteQueryAsync("Rules/Rename.sql", new { OldName = row.OriginalName, NewName = row.Name });
                row.MarkRenamed();
            }

            await _db.ExecuteQueryAsync("Rules/Update.sql", row.ToUpdateParam());
        }
    }

    /// <summary>Re-applies all Rules over the selected date range, then reloads.</summary>
    [RelayCommand]
    private async Task ExecuteRules()
    {
        if (!_db.IsOpen) return;

        var fromDate = FromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var toDate   = ToDate   ?? new DateTime(
            DateTime.Today.Year,
            DateTime.Today.Month,
            DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

        await _ruleEngine.ExecuteAsync(fromDate, toDate);
        await RefreshAsync();
    }

    /// <summary>Confirms with the user, then deletes the rule along with all Conditions that reference it.</summary>
    public async Task DeleteRuleAsync(RuleTreeNodeViewModel row)
    {
        if (!_db.IsOpen) return;

        var confirmDelete = await _dialogService.ConfirmAsync(
            "Delete Rule",
            $"Are you sure you want to delete the rule \"{row.Name}\"?\n\n" +
            "All Conditions on this rule will also be deleted.");
        if (!confirmDelete) return;

        await _db.ExecuteQueryAsync("Rules/Delete.sql", new { row.Name });

        await RefreshAsync();
    }
}