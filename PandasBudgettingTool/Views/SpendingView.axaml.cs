using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class SpendingView : UserControl
{
    public SpendingView()
    {
        InitializeComponent();
    }

    private void OnTransactionsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SpendingCategoryNodeViewModel row }) return;
        if (DataContext is not SpendingViewModel vm) return;

        vm.RequestOpenTransactionsForCategory(row.Name);
    }
}