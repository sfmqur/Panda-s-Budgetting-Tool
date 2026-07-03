using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

public partial class BudgetCategoryRowViewModel : ObservableObject
{
    /// <summary>Primary key — not editable after creation.</summary>
    public string Name { get; }

    [ObservableProperty]
    private string? _parent;

    [ObservableProperty]
    private bool _isExcludedFromSpendingTotal;

    /// <summary>Stored as string in the grid so the DataGrid can edit it as text. Empty = no target.</summary>
    [ObservableProperty]
    private string _budgetTarget = string.Empty;

    // Options for the Parent dropdown — every other category, plus empty string for "no parent"
    public IReadOnlyList<string?> ParentOptions { get; }

    public BudgetCategoryRowViewModel(BudgetCategory category, IReadOnlyList<string?> parentOptions)
    {
        ParentOptions                = parentOptions;
        Name                         = category.Name;
        _parent                      = category.Parent;
        _isExcludedFromSpendingTotal = category.IsExcludedFromSpendingTotal;
        _budgetTarget                = category.BudgetTarget?.ToString("F2") ?? string.Empty;
    }

    public object ToUpdateParam() => new
    {
        Name,
        Parent                       = string.IsNullOrWhiteSpace(Parent) ? null : Parent,
        IsExcludedFromSpendingTotal,
        BudgetTarget                 = decimal.TryParse(BudgetTarget, out var t) ? t : (decimal?)null
    };
}