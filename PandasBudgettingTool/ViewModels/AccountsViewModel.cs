using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class AccountsViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public AccountsViewModel(DatabaseService db) => _db = db;

    public ObservableCollection<AccountRowViewModel> Accounts { get; } = [];

    /// <summary>Raised when an account row's Transactions button is clicked — the argument is the Account's Name.</summary>
    public event Action<string>? OpenTransactionsForAccountRequested;

    public void RequestOpenTransactionsForAccount(string accountName) =>
        OpenTransactionsForAccountRequested?.Invoke(accountName);

    public override async Task RefreshAsync()
    {
        if (!_db.IsOpen) return;

        var rows = await _db.QueryAsync<Account>("Accounts/GetAll.sql");
        Accounts.Clear();
        foreach (var a in rows)
            Accounts.Add(new AccountRowViewModel(a));
    }

    public override async Task SaveAsync()
    {
        if (!_db.IsOpen) return;

        foreach (var row in Accounts)
            await _db.ExecuteQueryAsync("Accounts/Update.sql", row.ToUpdateParam());
    }
}