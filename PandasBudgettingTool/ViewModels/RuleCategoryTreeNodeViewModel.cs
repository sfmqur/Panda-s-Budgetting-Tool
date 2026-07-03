namespace PandasBudgettingTool.ViewModels;

public partial class RuleCategoryTreeNodeViewModel : RuleTreeNode
{
    public string Name { get; }

    public RuleCategoryTreeNodeViewModel(string name) => Name = name;
}