using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

public partial class ConditionRowViewModel : ObservableObject
{
    private static readonly string[] StringConditionals =
        ["Contains", "StartsWith", "EndsWith", "Equals", "NotEquals"];

    private static readonly string[] NumericConditionals =
        ["Equals", "NotEquals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual"];

    private static readonly string[] s_andOrOptions = Enum.GetNames<AndOr>();

    /// <summary>Primary key — not editable after creation.</summary>
    public string Id { get; }

    [ObservableProperty]
    private string _ruleName = string.Empty;

    [ObservableProperty]
    private decimal? _rank;

    [ObservableProperty]
    private string _andOr = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConditionalOptions))]
    private bool _isStringProperty;

    [ObservableProperty]
    private string _transactionProperty = string.Empty;

    [ObservableProperty]
    private string _conditional = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    // Conditional options depend on the property type — recomputed whenever IsStringProperty changes
    public string[] ConditionalOptions => IsStringProperty ? StringConditionals : NumericConditionals;

    // Shared option lists — held by reference so ComboBoxes stay in sync across rows
    public IReadOnlyList<string> RuleNames { get; }
    public IReadOnlyList<string> TransactionProperties { get; } =
        ["Name", "Amount", "Date", "Description", "AccountName", "BudgetCategoryName"];
    public IReadOnlyList<string> AndOrOptions => s_andOrOptions;

    public ConditionRowViewModel(Condition condition, IReadOnlyList<string> ruleNames)
    {
        RuleNames            = ruleNames;
        Id                   = condition.Id;
        _ruleName            = condition.RuleName;
        _rank                = condition.Rank;
        _andOr               = condition.AndOr;
        _isStringProperty    = condition.IsStringProperty;
        _transactionProperty = condition.TransactionProperty;
        _conditional         = condition.Conditional;
        _value               = condition.Value;
    }

    // When the property type changes, clear the conditional since the valid options change
    partial void OnIsStringPropertyChanged(bool value) => Conditional = string.Empty;

    public object ToUpdateParam() => new
    {
        Id,
        RuleName,
        Rank = (int)(Rank ?? 0),
        AndOr,
        IsStringProperty,
        TransactionProperty,
        Conditional,
        Value
    };
}