using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PandasBudgettingTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
    private bool _canNavigateBack;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateForwardCommand))]
    private bool _canNavigateForward;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private string _currentDatabasePath = string.Empty;

    // ── File Menu ────────────────────────────────────────────────────────────

    [RelayCommand]
    private Task FileNew() => Task.CompletedTask;

    [RelayCommand]
    private Task FileOpen() => Task.CompletedTask;

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