using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PandasBudgettingTool.ViewModels;

/// <summary>Base type for nodes in the Rules tree — either a RuleCategory or a leaf Rule.</summary>
public abstract partial class RuleTreeNode : ObservableObject
{
    // Empty for leaf Rule nodes — populated with child categories then child rules for category nodes.
    public ObservableCollection<RuleTreeNode> Children { get; } = [];
}