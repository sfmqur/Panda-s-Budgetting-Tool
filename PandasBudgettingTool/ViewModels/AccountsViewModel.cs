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
    private readonly DialogService   _dialogService;

    public AccountsViewModel(DatabaseService db, DialogService dialogService)
    {
        _db            = db;
        _dialogService = dialogService;
    }

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

    /// <summary>
    /// Confirms with the user, then deletes the account — either deleting its transactions
    /// or clearing their AccountName, per the user's second-dialog choice.
    /// </summary>
    public async Task DeleteAccountAsync(string accountName)
    {
        if (!_db.IsOpen) return;

        var confirmDelete = await _dialogService.ConfirmAsync(
            "Delete Account",
            $"Are you sure you want to delete the account \"{accountName}\"?");
        if (!confirmDelete) return;

        var deleteTransactions = await _dialogService.ConfirmAsync(
            "Delete Transactions",
            $"Would you also like to delete the transactions associated with \"{accountName}\"?\n\n" +
            "Choosing \"No\" keeps the transactions but clears their account.");

        if (deleteTransactions)
            await _db.ExecuteQueryAsync("Transactions/DeleteByAccount.sql", new { AccountName = accountName });
        else
            await _db.ExecuteQueryAsync("Transactions/ClearAccountName.sql", new { AccountName = accountName });

        await _db.ExecuteQueryAsync("Accounts/Delete.sql", new { Name = accountName });

        await RefreshAsync();
    }
}