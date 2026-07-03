using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

/// <summary>Leaf node in the Rules tree — a single Rule with its editable properties (Conditions are edited elsewhere).</summary>
public partial class RuleTreeNodeViewModel : RuleTreeNode
{
    /// <summary>Name as loaded from the database — used to detect a rename on Save.</summary>
    public string OriginalName { get; private set; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private decimal? _rank;

    [ObservableProperty]
    private string? _budgetCategoryName;

    [ObservableProperty]
    private string? _ruleCategoryName;

    // Shared option lists — held by reference so ComboBoxes stay in sync across rows
    public IReadOnlyList<string?> BudgetCategoryOptions { get; }
    public IReadOnlyList<string?> RuleCategoryOptions   { get; }

    public RuleTreeNodeViewModel(Rule rule, IReadOnlyList<string?> budgetCategoryOptions, IReadOnlyList<string?> ruleCategoryOptions)
    {
        BudgetCategoryOptions = budgetCategoryOptions;
        RuleCategoryOptions   = ruleCategoryOptions;
        OriginalName          = rule.Name;
        _name                 = rule.Name;
        _rank                 = rule.Rank;
        _budgetCategoryName   = rule.BudgetCategoryName;
        _ruleCategoryName     = rule.RuleCategoryName;
    }

    /// <summary>Call after the rename has been persisted so a later Save doesn't try to rename it again.</summary>
    public void MarkRenamed() => OriginalName = Name;

    public object ToUpdateParam() => new
    {
        Name,
        Rank                = (int)(Rank ?? 0),
        BudgetCategoryName,
        RuleCategoryName
    };
}