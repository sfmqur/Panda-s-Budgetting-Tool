using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ConfigService _configService;
    private readonly DatabaseService _databaseService;
    private readonly DialogService _dialogService;

    // Design-time constructor
    public MainWindowViewModel()
        : this(new ConfigService(), new DatabaseService(), new DialogService()) { }

    public MainWindowViewModel(ConfigService configService, DatabaseService databaseService, DialogService dialogService)
    {
        _configService = configService;
        _databaseService = databaseService;
        _dialogService = dialogService;
    }

    // ── Properties ───────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
    private bool _canNavigateBack;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateForwardCommand))]
    private bool _canNavigateForward;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private string _currentDatabasePath = string.Empty;

    public string WindowTitle => string.IsNullOrEmpty(CurrentDatabasePath)
        ? "Panda's Budgeting Tool"
        : $"Panda's Budgeting Tool — {Path.GetFileName(CurrentDatabasePath)}";

    // ── Startup / shutdown ───────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        var config = await _configService.LoadAsync();
        if (!string.IsNullOrEmpty(config.LastOpenedDatabasePath)
            && File.Exists(config.LastOpenedDatabasePath))
        {
            await _databaseService.OpenAsync(config.LastOpenedDatabasePath);
            CurrentDatabasePath = config.LastOpenedDatabasePath;
        }
    }

    public Task SaveConfigAsync() =>
        _configService.SaveAsync(new ConfigDto { LastOpenedDatabasePath = CurrentDatabasePath });

    // ── File Menu ────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task FileNew()
    {
        var path = await _dialogService.PickNewDatabaseFileAsync();
        if (string.IsNullOrEmpty(path)) return;

        await _databaseService.CreateNewAsync(path);
        CurrentDatabasePath = path;
        await SaveConfigAsync();
    }

    [RelayCommand]
    private async Task FileOpen()
    {
        var path = await _dialogService.PickOpenDatabaseFileAsync();
        if (string.IsNullOrEmpty(path)) return;

        await _databaseService.OpenAsync(path);
        CurrentDatabasePath = path;
        await SaveConfigAsync();
    }

    [RelayCommand]
    private void FileSave() { }

    [RelayCommand]
    private void Exit() { }

    // ── Navigation ───────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack() { }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    private void NavigateForward() { }

    [RelayCommand]
    private void NavigateToTransactions() { }

    [RelayCommand]
    private void NavigateToAccounts() { }

    [RelayCommand]
    private void NavigateToBudget() { }

    [RelayCommand]
    private void NavigateToSpending() { }

    [RelayCommand]
    private void NavigateToRules() { }

    // ── Import Menu ──────────────────────────────────────────────────────────

    [RelayCommand]
    private Task ImportStatement() => Task.CompletedTask;
}