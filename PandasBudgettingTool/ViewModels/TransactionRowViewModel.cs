using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

public partial class TransactionRowViewModel : ObservableObject
{
    /// <summary>Composite PK — not editable (Date|Name|Amount at import time).</summary>
    public string Id { get; }

    /// <summary>ISO-8601 date string (YYYY-MM-DD) for direct grid editing.</summary>
    [ObservableProperty]
    private string _date = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>Stored as string in the grid so the DataGrid can edit it as text.</summary>
    [ObservableProperty]
    private string _amount = string.Empty;

    [ObservableProperty]
    private string? _budgetCategoryName;

    [ObservableProperty]
    private bool _userAdjustedCategory;

    [ObservableProperty]
    private string? _accountName;

    // Shared option lists — held by reference so ComboBoxes stay in sync if options refresh
    public IReadOnlyList<string?> BudgetCategoryOptions { get; }
    public IReadOnlyList<string?> AccountOptions { get; }

    public TransactionRowViewModel(
        Transaction t,
        IReadOnlyList<string?> budgetCategoryOptions,
        IReadOnlyList<string?> accountOptions)
    {
        BudgetCategoryOptions = budgetCategoryOptions;
        AccountOptions        = accountOptions;
        Id                    = t.Id;
        _date                 = t.Date.ToString("yyyy-MM-dd");
        _name                 = t.Name;
        _description          = t.Description;
        _amount               = t.Amount.ToString("F2");
        _budgetCategoryName   = t.BudgetCategoryName;
        _userAdjustedCategory = t.UserAdjustedCategory;
        _accountName          = t.AccountName;
    }

    public object ToUpdateParam() => new
    {
        Id,
        Date                 = Date,
        Name,
        Description,
        Amount               = decimal.TryParse(Amount, out var a) ? a : 0m,
        BudgetCategoryName   = string.IsNullOrWhiteSpace(BudgetCategoryName) ? null : BudgetCategoryName,
        UserAdjustedCategory,
        AccountName          = string.IsNullOrWhiteSpace(AccountName) ? null : AccountName
    };
}