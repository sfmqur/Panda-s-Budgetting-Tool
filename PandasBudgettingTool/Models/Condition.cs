namespace PandasBudgettingTool.Models;

public class Condition
{
    /// <summary>GUID stored as TEXT. Use Guid.NewGuid().ToString() when creating.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>References Rule.Name. Cascade-deletes when the parent Rule is removed.</summary>
    public string RuleName { get; set; } = string.Empty;
    /// <summary>Execution order among a Rule's Conditions — lowest evaluated first.</summary>
    public int Rank { get; set; }
    /// <summary>How this Condition combines with the previous one's result: "And" or "Or" (see Models.AndOr).</summary>
    public string AndOr { get; set; } = "Or";
    /// <summary>True = compare as string; false = compare as numeric.</summary>
    public bool IsStringProperty { get; set; } = true;
    /// <summary>Name of the Transaction property to test (e.g. "Name", "Amount", "Date").</summary>
    public string TransactionProperty { get; set; } = string.Empty;
    /// <summary>Comparison operator (e.g. "Contains", "Equals", "GreaterThan", "LessThan").</summary>
    public string Conditional { get; set; } = string.Empty;
    /// <summary>The value to compare against, stored as text regardless of comparison type.</summary>
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{RuleName} -  {TransactionProperty} {Conditional} {Value}";
    }
}