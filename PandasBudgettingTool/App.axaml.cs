using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PandasBudgettingTool.Services;
using PandasBudgettingTool.ViewModels;
using PandasBudgettingTool.Views;

namespace PandasBudgettingTool;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var configService  = new ConfigService();
            var databaseService = new DatabaseService();
            var dialogService  = new DialogService();
            var ruleEngine     = new RuleEngine(databaseService);

            var vm = new MainWindowViewModel(configService, databaseService, dialogService, ruleEngine);

            desktop.MainWindow = new MainWindow { DataContext = vm };

            base.OnFrameworkInitializationCompleted();

            await vm.InitializeAsync();
        }
        else
        {
            base.OnFrameworkInitializationCompleted();
        }
    }
}