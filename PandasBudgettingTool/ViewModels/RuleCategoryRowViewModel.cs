using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

public partial class RuleCategoryRowViewModel : ObservableObject
{
    /// <summary>Primary key — not editable after creation.</summary>
    public string Name { get; }

    [ObservableProperty]
    private string? _parentRuleCategoryName;

    // Options for the Parent dropdown — every other rule category, plus empty string for "no parent"
    public IReadOnlyList<string?> ParentOptions { get; }

    public RuleCategoryRowViewModel(RuleCategory category, IReadOnlyList<string?> parentOptions)
    {
        ParentOptions          = parentOptions;
        Name                   = category.Name;
        _parentRuleCategoryName = category.ParentRuleCategoryName;
    }

    public object ToUpdateParam() => new
    {
        Name,
        ParentRuleCategoryName = string.IsNullOrWhiteSpace(ParentRuleCategoryName) ? null : ParentRuleCategoryName
    };
}