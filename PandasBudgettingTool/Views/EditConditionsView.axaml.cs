using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class EditConditionsView : UserControl
{
    public EditConditionsView()
    {
        InitializeComponent();
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: ConditionRowViewModel row }) return;
        if (DataContext is not EditConditionsViewModel vm) return;

        await vm.DeleteConditionAsync(row);
    }
}