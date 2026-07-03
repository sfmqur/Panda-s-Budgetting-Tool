using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.ViewModels;

public partial class AccountRowViewModel : ObservableObject
{
    /// <summary>Primary key — not editable after creation.</summary>
    public string Name { get; }

    [ObservableProperty]
    private bool _isMinusSignAnExpense;

    [ObservableProperty]
    private string _importerType = string.Empty;

    // Options list shared by all rows — empty string = no importer assigned
    private static readonly IReadOnlyList<string> s_importerTypes =
        [string.Empty, .. Enum.GetNames<Importers>()];

    public IReadOnlyList<string> ImporterTypes => s_importerTypes;

    public AccountRowViewModel(Account account)
    {
        Name                 = account.Name;
        _isMinusSignAnExpense = account.IsMinusSignAnExpense;
        _importerType        = account.ImporterType;
    }

    public object ToUpdateParam() => new
    {
        Name,
        IsMinusSignAnExpense,
        ImporterType
    };
}