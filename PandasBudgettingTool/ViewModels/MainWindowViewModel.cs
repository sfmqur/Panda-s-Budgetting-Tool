using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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

    private readonly Stack<ViewModelBase> _backStack    = new();
    private readonly Stack<ViewModelBase> _forwardStack = new();

    // Cached page instances so state is preserved across navigation
    private TransactionsViewModel? _transactionsVm;
    private AccountsViewModel?     _accountsVm;
    private BudgetViewModel?       _budgetVm;
    private SpendingViewModel?     _spendingVm;
    private RulesViewModel?        _rulesVm;

    // Design-time constructor
    public MainWindowViewModel()
        : this(new ConfigService(), new DatabaseService(), new DialogService()) { }

    public MainWindowViewModel(ConfigService configService, DatabaseService databaseService, DialogService dialogService)
    {
        _configService   = configService;
        _databaseService = databaseService;
        _dialogService   = dialogService;
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
    private void Exit()
    {
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.Shutdown();
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack()
    {
        var page = _backStack.Pop();
        _forwardStack.Push(CurrentPage!);
        CurrentPage = page;
        UpdateNavigationState();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    private void NavigateForward()
    {
        var page = _forwardStack.Pop();
        _backStack.Push(CurrentPage!);
        CurrentPage = page;
        UpdateNavigationState();
    }

    [RelayCommand]
    private void NavigateToTransactions() =>
        NavigateTo(_transactionsVm ??= new TransactionsViewModel());

    [RelayCommand]
    private void NavigateToAccounts() =>
        NavigateTo(_accountsVm ??= new AccountsViewModel());

    [RelayCommand]
    private void NavigateToBudget() =>
        NavigateTo(_budgetVm ??= new BudgetViewModel());

    [RelayCommand]
    private void NavigateToSpending() =>
        NavigateTo(_spendingVm ??= new SpendingViewModel());

    [RelayCommand]
    private void NavigateToRules() =>
        NavigateTo(_rulesVm ??= new RulesViewModel());

    // ── Import Menu ──────────────────────────────────────────────────────────

    [RelayCommand]
    private Task ImportStatement() => Task.CompletedTask;

    // ── Private helpers ──────────────────────────────────────────────────────

    private void NavigateTo(ViewModelBase page)
    {
        if (ReferenceEquals(CurrentPage, page)) return;

        if (CurrentPage is not null)
            _backStack.Push(CurrentPage);

        _forwardStack.Clear();
        CurrentPage = page;
        UpdateNavigationState();
    }

    private void UpdateNavigationState()
    {
        CanNavigateBack    = _backStack.Count > 0;
        CanNavigateForward = _forwardStack.Count > 0;
    }
}