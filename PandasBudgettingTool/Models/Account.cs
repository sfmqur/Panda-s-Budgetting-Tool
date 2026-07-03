namespace PandasBudgettingTool.Models;

public class Account
{
    public string Name { get; set; } = string.Empty;
    /// <summary>When true, a negative amount in the import file is treated as an expense.</summary>
    public bool IsMinusSignAnExpense { get; set; } = true;
    /// <summary>Key into the importer factory (e.g. "Csv", "Ofx").</summary>
    public string ImporterType { get; set; } = string.Empty;
}