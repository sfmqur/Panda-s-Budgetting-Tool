using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

public partial class RuleCategoryRowViewModel : ObservableObject
{
    /// <summary>Name as loaded from the database — used to detect a rename on Save.</summary>
    public string OriginalName { get; private set; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string? _parentRuleCategoryName;

    // Options for the Parent dropdown — every other rule category, plus empty string for "no parent"
    public IReadOnlyList<string?> ParentOptions { get; }

    public RuleCategoryRowViewModel(RuleCategory category, IReadOnlyList<string?> parentOptions)
    {
        ParentOptions           = parentOptions;
        OriginalName            = category.Name;
        _name                   = category.Name;
        _parentRuleCategoryName = category.ParentRuleCategoryName;
    }

    /// <summary>Call after the rename has been persisted so a later Save doesn't try to rename it again.</summary>
    public void MarkRenamed() => OriginalName = Name;

    public object ToUpdateParam() => new
    {
        Name,
        ParentRuleCategoryName = string.IsNullOrWhiteSpace(ParentRuleCategoryName) ? null : ParentRuleCategoryName
    };
}