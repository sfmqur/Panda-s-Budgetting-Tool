using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PandasBudgettingTool.Views;

public partial class RenameDialogWindow : Window
{
    public RenameDialogWindow()
    {
        InitializeComponent();
    }

    public RenameDialogWindow(string title, string message, string currentName) : this()
    {
        Title = title;
        MessageText.Text = message;
        NameTextBox.Text = currentName;
        NameTextBox.SelectAll();
    }

    private void OnOkClick(object? sender, RoutedEventArgs e) => Close(NameTextBox.Text);

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(null);
}