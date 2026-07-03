using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class AccountsView : UserControl
{
    public AccountsView()
    {
        InitializeComponent();
    }

    private void OnTransactionsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: AccountRowViewModel row }) return;
        if (DataContext is not AccountsViewModel vm) return;

        vm.RequestOpenTransactionsForAccount(row.Name);
    }
}