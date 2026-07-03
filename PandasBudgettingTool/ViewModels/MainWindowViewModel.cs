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
    private readonly ConfigService   _configService;
    private readonly DatabaseService _databaseService;
    private readonly DialogService   _dialogService;

    private readonly Stack<ViewModelBase> _backStack    = new();
    private readonly Stack<ViewModelBase> _forwardStack = new();

    // ── Cached view pages ────────────────────────────────────────────────────
    private TransactionsViewModel? _transactionsVm;
    private AccountsViewModel?     _accountsVm;
    private BudgetViewModel?       _budgetVm;
    private SpendingViewModel?     _spendingVm;
    private RulesViewModel?        _rulesVm;
    private EditBudgetCategoriesViewModel? _editBudgetCategoriesVm;
    private EditRuleCategoriesViewModel?   _editRuleCategoriesVm;
    private EditConditionsViewModel?       _editConditionsVm;

    // ── Cached create pages ──────────────────────────────────────────────────
    private CreateTransactionViewModel?  _createTransactionVm;
    private CreateAccountViewModel?      _createAccountVm;
    private CreateBudgetCategoryViewModel? _createBudgetCategoryVm;
    private CreateRuleViewModel?         _createRuleVm;
    private CreateRuleCategoryViewModel? _createRuleCategoryVm;
    private CreateConditionViewModel?    _createConditionVm;
    private ImportStatementViewModel?    _importStatementVm;

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
    private async Task FileSave()
    {
        if (CurrentPage is not null)
            await CurrentPage.SaveAsync();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (CurrentPage is not null)
            await CurrentPage.RefreshAsync();
    }

    [RelayCommand]
    private void Exit()
    {
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.Shutdown();
    }

    // ── View Navigation ──────────────────────────────────────────────────────

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
    private async Task NavigateToTransactions()
    {
        _transactionsVm ??= new TransactionsViewModel(_databaseService);
        NavigateTo(_transactionsVm);
        await _transactionsVm.RefreshAsync();
    }

    [RelayCommand]
    private async Task NavigateToAccounts()
    {
        if (_accountsVm is null)
        {
            _accountsVm = new AccountsViewModel(_databaseService);
            _accountsVm.OpenTransactionsForAccountRequested += OnOpenTransactionsForAccountRequested;
        }

        NavigateTo(_accountsVm);
        await _accountsVm.RefreshAsync();
    }

    private async void OnOpenTransactionsForAccountRequested(string accountName)
    {
        _transactionsVm ??= new TransactionsViewModel(_databaseService);
        NavigateTo(_transactionsVm);
        await _transactionsVm.FilterToAccountAsync(accountName);
    }

    [RelayCommand]
    private async Task NavigateToEditBudgetCategories()
    {
        _editBudgetCategoriesVm ??= new EditBudgetCategoriesViewModel(_databaseService);
        NavigateTo(_editBudgetCategoriesVm);
        await _editBudgetCategoriesVm.RefreshAsync();
    }

    [RelayCommand]
    private async Task NavigateToEditRuleCategories()
    {
        _editRuleCategoriesVm ??= new EditRuleCategoriesViewModel(_databaseService);
        NavigateTo(_editRuleCategoriesVm);
        await _editRuleCategoriesVm.RefreshAsync();
    }

    [RelayCommand]
    private async Task NavigateToEditConditions()
    {
        _editConditionsVm ??= new EditConditionsViewModel(_databaseService);
        NavigateTo(_editConditionsVm);
        await _editConditionsVm.RefreshAsync();
    }

    [RelayCommand]
    private void NavigateToBudget() =>
        NavigateTo(_budgetVm ??= new BudgetViewModel());

    [RelayCommand]
    private void NavigateToSpending() =>
        NavigateTo(_spendingVm ??= new SpendingViewModel());

    [RelayCommand]
    private async Task NavigateToRules()
    {
        if (_rulesVm is null)
        {
            _rulesVm = new RulesViewModel(_databaseService);
            _rulesVm.OpenRuleConditionsRequested += OnOpenRuleConditionsRequested;
        }

        NavigateTo(_rulesVm);
        await _rulesVm.RefreshAsync();
    }

    private async void OnOpenRuleConditionsRequested(string ruleName)
    {
        _editConditionsVm ??= new EditConditionsViewModel(_databaseService);
        await _editConditionsVm.RefreshAsync();
        _editConditionsVm.SelectedRuleFilter = ruleName;
        NavigateTo(_editConditionsVm);
    }

    // ── Create Navigation ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task NavigateToCreateTransaction()
    {
        _createTransactionVm ??= new CreateTransactionViewModel(_databaseService);
        await _createTransactionVm.LoadOptionsAsync();
        NavigateTo(_createTransactionVm);
    }

    [RelayCommand]
    private async Task NavigateToCreateAccount()
    {
        _createAccountVm ??= new CreateAccountViewModel(_databaseService);
        NavigateTo(_createAccountVm);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NavigateToCreateBudgetCategory()
    {
        _createBudgetCategoryVm ??= new CreateBudgetCategoryViewModel(_databaseService);
        await _createBudgetCategoryVm.LoadOptionsAsync();
        NavigateTo(_createBudgetCategoryVm);
    }

    [RelayCommand]
    private async Task NavigateToCreateRule()
    {
        _createRuleVm ??= new CreateRuleViewModel(_databaseService);
        await _createRuleVm.LoadOptionsAsync();
        NavigateTo(_createRuleVm);
    }

    [RelayCommand]
    private async Task NavigateToCreateRuleCategory()
    {
        _createRuleCategoryVm ??= new CreateRuleCategoryViewModel(_databaseService);
        await _createRuleCategoryVm.LoadOptionsAsync();
        NavigateTo(_createRuleCategoryVm);
    }

    [RelayCommand]
    private async Task NavigateToCreateCondition()
    {
        _createConditionVm ??= new CreateConditionViewModel(_databaseService);
        await _createConditionVm.LoadOptionsAsync();
        NavigateTo(_createConditionVm);
    }

    // ── Import Menu ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task ImportStatement()
    {
        _importStatementVm ??= new ImportStatementViewModel(_databaseService, _dialogService);
        NavigateTo(_importStatementVm);
        await _importStatementVm.RefreshAsync();
    }

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