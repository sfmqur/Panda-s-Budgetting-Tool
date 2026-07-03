using Avalonia.Controls;
using Avalonia.Interactivity;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class RulesView : UserControl
{
    public RulesView()
    {
        InitializeComponent();
    }

    private void OnConditionsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: RuleTreeNodeViewModel rule }) return;
        if (DataContext is not RulesViewModel vm) return;

        vm.RequestOpenRuleConditions(rule.Name);
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: RuleTreeNodeViewModel rule }) return;
        if (DataContext is not RulesViewModel vm) return;

        await vm.DeleteRuleAsync(rule);
    }
}