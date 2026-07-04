using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

/// <summary>One node in the Spending page's read-only category tree.</summary>
public partial class SpendingCategoryNodeViewModel : ObservableObject
{
    public string Name { get; }

    /// <summary>This category's own saved target — used only to compute the rolled-up BudgetTarget below.</summary>
    public decimal OwnBudgetTarget { get; }

    /// <summary>Actual spend for this category and all descendants over the selected date range.</summary>
    [ObservableProperty]
    private decimal _spendingForTerm;

    /// <summary>OwnBudgetTarget + every descendant's own budget target, recursively.</summary>
    [ObservableProperty]
    private decimal _budgetTarget;

    /// <summary>BudgetTarget - SpendingForTerm.</summary>
    [ObservableProperty]
    private decimal _remaining;

    public ObservableCollection<SpendingCategoryNodeViewModel> Children { get; } = [];

    public SpendingCategoryNodeViewModel(BudgetCategory category)
    {
        Name            = category.Name;
        OwnBudgetTarget = category.BudgetTarget ?? 0;
    }
}