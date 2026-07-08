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
    private readonly DatabaseService _db;
    private readonly DialogService   _dialogService;
    private readonly RuleEngine      _ruleEngine;
    private List<RuleTreeNodeViewModel> _allRuleNodes = [];

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

    public ObservableCollection<RuleTreeNode> RootNodes { get; } = [];

    /// <summary>Raised when a Rule row is double-clicked — the argument is the Rule's Name.</summary>
    public event Action<string>? OpenRuleConditionsRequested;

    public void RequestOpenRuleConditions(string ruleName) => OpenRuleConditionsRequested?.Invoke(ruleName);

    public override async Task RefreshAsync()
    {
        RootNodes.Clear();
        if (!_db.IsOpen) return;

        var categories = (await _db.QueryAsync<RuleCategory>("RuleCategories/GetAll.sql")).ToList();
        var rules      = (await _db.QueryAsync<Rule>("Rules/GetAll.sql")).ToList();

        var budgetCategoryOptions = new List<string?> { null };
        budgetCategoryOptions.AddRange(await _db.QueryAsync<string>("BudgetCategories/GetAllNames.sql"));

        var ruleCategoryOptions = new List<string?> { null };
        ruleCategoryOptions.AddRange(categories.Select(c => c.Name));

        _allRuleNodes = rules
            .Select(r => new RuleTreeNodeViewModel(r, budgetCategoryOptions, ruleCategoryOptions))
            .ToList();

        var categoryNodes = categories.ToDictionary(
            c => c.Name,
            c => new RuleCategoryTreeNodeViewModel(c.Name));

        // Sub-categories first …
        foreach (var cat in categories)
        {
            if (cat.ParentRuleCategoryName is not null
                && categoryNodes.TryGetValue(cat.ParentRuleCategoryName, out var parentNode))
            {
                parentNode.Children.Add(categoryNodes[cat.Name]);
            }
        }

        // … then the rules that belong directly to each category.
        foreach (var ruleNode in _allRuleNodes)
        {
            if (ruleNode.RuleCategoryName is not null
                && categoryNodes.TryGetValue(ruleNode.RuleCategoryName, out var parentNode))
            {
                parentNode.Children.Add(ruleNode);
            }
        }

        foreach (var cat in categories.Where(c => c.ParentRuleCategoryName is null))
            RootNodes.Add(categoryNodes[cat.Name]);

        foreach (var ruleNode in _allRuleNodes.Where(r => r.RuleCategoryName is null))
            RootNodes.Add(ruleNode);
    }

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