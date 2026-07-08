using System.Collections.Generic;

namespace PandasBudgettingTool.Models;

public class Rule
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Evaluation order; lower rank runs first.</summary>
    public int Rank { get; set; }
    /// <summary>The BudgetCategory to assign when all Conditions match. Null = no assignment.</summary>
    public string? BudgetCategoryName { get; set; }
    /// <summary>Optional grouping; references RuleCategory.Name.</summary>
    public string? RuleCategoryName { get; set; }
    /// <summary>
    /// Conditions that must all pass for this rule to fire.
    /// Not stored on the Rule row — populated by querying the Condition table via RuleName FK.
    /// </summary>
    public List<Condition> Conditions { get; set; } = [];

    public override string ToString()
    {
        return $"{Name} - {RuleCategoryName}";
    }
}