using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class EditBudgetCategoriesView : UserControl
{
    public EditBudgetCategoriesView()
    {
        InitializeComponent();
    }

    private async void OnRenameClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: BudgetCategoryRowViewModel row }) return;
        if (DataContext is not EditBudgetCategoriesViewModel vm) return;

        await vm.RenameBudgetCategoryAsync(row);
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: BudgetCategoryRowViewModel row }) return;
        if (DataContext is not EditBudgetCategoriesViewModel vm) return;

        await vm.DeleteBudgetCategoryAsync(row);
    }
}