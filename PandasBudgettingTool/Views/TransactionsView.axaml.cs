using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class TransactionsView : UserControl
{
    public TransactionsView()
    {
        InitializeComponent();
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: TransactionRowViewModel row }) return;
        if (DataContext is not TransactionsViewModel vm) return;

        await vm.DeleteTransactionAsync(row);
    }
}