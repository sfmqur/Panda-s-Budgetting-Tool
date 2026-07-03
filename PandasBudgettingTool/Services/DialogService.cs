using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using PandasBudgettingTool.Views;

namespace PandasBudgettingTool.Services;

public class DialogService
{
    private static readonly FilePickerFileType DbFileType = new("SQLite Database")
    {
        Patterns = ["*.db", "*.sqlite", "*.sqlite3"]
    };

    private static readonly FilePickerFileType StatementFileType = new("Statement Files")
    {
        Patterns = ["*.csv", "*.ofx", "*.qfx", "*.txt"]
    };

    public async Task<string?> PickNewDatabaseFileAsync()
    {
        var provider = GetStorageProvider();
        if (provider is null) return null;

        var result = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Create New Budget Database",
            SuggestedFileName = "budget",
            DefaultExtension = "db",
            FileTypeChoices = [DbFileType]
        });

        return result?.Path.LocalPath;
    }

    public async Task<string?> PickOpenDatabaseFileAsync()
    {
        var provider = GetStorageProvider();
        if (provider is null) return null;

        var results = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Budget Database",
            AllowMultiple = false,
            FileTypeFilter = [DbFileType]
        });

        return results.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> PickTransactionFileAsync()
    {
        var provider = GetStorageProvider();
        if (provider is null) return null;

        var results = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Transaction Data",
            AllowMultiple = false,
            FileTypeFilter = [StatementFileType, FilePickerFileTypes.All]
        });

        return results.FirstOrDefault()?.Path.LocalPath;
    }

    /// <summary>Shows a Yes/No confirmation dialog and returns true if the user chose Yes.</summary>
    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var owner = GetMainWindow();
        if (owner is null) return false;

        var dialog = new ConfirmDialogWindow(title, message);
        var result = await dialog.ShowDialog<bool?>(owner);
        return result ?? false;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return TopLevel.GetTopLevel(desktop.MainWindow)?.StorageProvider;
        return null;
    }

    private static Window? GetMainWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
}