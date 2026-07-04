using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

/// <summary>One node in the Budget page's category tree.</summary>
public partial class BudgetCategoryNodeViewModel : ObservableObject
{
    public string Name { get; }

    /// <summary>This category's own saved target — the only field this page persists.</summary>
    [ObservableProperty]
    private decimal? _ownBudgetTarget;

    /// <summary>Recursive sum of every descendant's own budget target.</summary>
    [ObservableProperty]
    private decimal _childrenBudgetSum;

    /// <summary>OwnBudgetTarget + ChildrenBudgetSum.</summary>
    [ObservableProperty]
    private decimal _totalBudget;

    /// <summary>Actual spend for this category and all descendants over the selected range, normalized to a 30-day rate.</summary>
    [ObservableProperty]
    private decimal _averageSpend;

    public ObservableCollection<BudgetCategoryNodeViewModel> Children { get; } = [];

    public BudgetCategoryNodeViewModel(BudgetCategory category)
    {
        Name             = category.Name;
        _ownBudgetTarget = category.BudgetTarget;
    }

    public object ToUpdateParam() => new { Name, BudgetTarget = OwnBudgetTarget };
}