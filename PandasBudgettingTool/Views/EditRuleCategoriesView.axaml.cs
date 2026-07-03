using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class EditRuleCategoriesView : UserControl
{
    public EditRuleCategoriesView()
    {
        InitializeComponent();
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: RuleCategoryRowViewModel row }) return;
        if (DataContext is not EditRuleCategoriesViewModel vm) return;

        await vm.DeleteRuleCategoryAsync(row);
    }
}