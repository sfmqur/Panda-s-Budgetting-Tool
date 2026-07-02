using Avalonia.Controls;
using PandasBudgettingTool.ViewModels;

namespace PandasBudgettingTool.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (DataContext is MainWindowViewModel vm)
            await vm.SaveConfigAsync();
    }
}