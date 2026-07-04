using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class BudgetView : UserControl
{
    public BudgetView()
    {
        InitializeComponent();
    }

    private void OnTransactionsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: BudgetCategoryNodeViewModel row }) return;
        if (DataContext is not BudgetViewModel vm) return;

        vm.RequestOpenTransactionsForCategory(row.Name);
    }
}