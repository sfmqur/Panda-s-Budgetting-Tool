using System;

namespace PandasBudgettingTool.Models;

public class Transaction
{
    /// <summary>Composite key built as "Date|Name|Amount" to detect duplicates on import.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Stored as ISO-8601 text (YYYY-MM-DD) in SQLite.</summary>
    public DateTime Date { get; set; }
    /// <summary>Name as it appears in the import file — not user-editable.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>User-editable free-text note.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Negative = expense, positive = income.</summary>
    public decimal Amount { get; set; }
    /// <summary>Category assigned by the rules engine. References BudgetCategory.Name.</summary>
    public string? BudgetCategoryName { get; set; }
    /// <summary>
    /// True when the user has manually set the category.
    /// The rules engine skips re-categorization for flagged transactions.
    /// </summary>
    public bool UserAdjustedCategory { get; set; }
    /// <summary>References Account.Name.</summary>
    public string? AccountName { get; set; }
}