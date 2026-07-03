using CommunityToolkit.Mvvm.ComponentModel;

namespace PandasBudgettingTool.ViewModels;

/// <summary>One entry in the Transaction filter's account checklist.</summary>
public partial class AccountFilterItem : ObservableObject
{
    public string Name { get; init; } = string.Empty;

    /// <summary>True for the special "(No Account)" row that matches NULL AccountName.</summary>
    public bool IsNoAccount { get; init; }

    public string DisplayName => IsNoAccount ? "(No Account)" : Name;

    [ObservableProperty]
    private bool _isSelected = true;
}