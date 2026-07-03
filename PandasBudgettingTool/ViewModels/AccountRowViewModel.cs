using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Models.Importing;

namespace PandasBudgettingTool.ViewModels;

public partial class AccountRowViewModel : ObservableObject
{
    /// <summary>Name as loaded from the database — used to detect a rename on Save.</summary>
    public string OriginalName { get; private set; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private bool _isMinusSignAnExpense;

    [ObservableProperty]
    private string _importerType = string.Empty;

    // Options list shared by all rows — empty string = no importer assigned
    private static readonly IReadOnlyList<string> s_importerTypes =
        [string.Empty, .. Enum.GetNames<ImporterType>()];

    public IReadOnlyList<string> ImporterTypes => s_importerTypes;

    public AccountRowViewModel(Account account)
    {
        OriginalName          = account.Name;
        _name                 = account.Name;
        _isMinusSignAnExpense = account.IsMinusSignAnExpense;
        _importerType         = account.ImporterType;
    }

    /// <summary>Call after the rename has been persisted so a later Save doesn't try to rename it again.</summary>
    public void MarkRenamed() => OriginalName = Name;

    public object ToUpdateParam() => new
    {
        Name,
        IsMinusSignAnExpense,
        ImporterType
    };
}