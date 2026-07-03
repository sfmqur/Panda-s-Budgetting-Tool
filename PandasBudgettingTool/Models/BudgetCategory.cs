namespace PandasBudgettingTool.Models;

public class BudgetCategory
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Null for top-level categories; references another BudgetCategory.Name.</summary>
    public string? Parent { get; set; }
    /// <summary>When true this category is omitted from spending totals (e.g. transfers).</summary>
    public bool IsExcludedFromSpendingTotal { get; set; }
    /// <summary>Optional monthly spending target. Null means no target is set.</summary>
    public decimal? BudgetTarget { get; set; }
}