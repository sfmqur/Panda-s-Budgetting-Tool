namespace PandasBudgettingTool.Models;

public class RuleCategory
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Null for top-level categories; references another RuleCategory.Name.</summary>
    public string? ParentRuleCategoryName { get; set; }
}