using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class CreateBudgetCategoryViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public CreateBudgetCategoryViewModel(DatabaseService db) => _db = db;

    public ObservableCollection<string> ExistingCategories { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _selectedParent;

    [ObservableProperty]
    private bool _isExcludedFromSpendingTotal;

    [ObservableProperty]
    private decimal? _budgetTarget;

    public async Task LoadOptionsAsync()
    {
        if (!_db.IsOpen) return;
        ExistingCategories.Clear();
        foreach (var n in await _db.QueryAsync<string>("BudgetCategories/GetAllNames.sql"))
            ExistingCategories.Add(n);
    }

    private bool CanCreate() => !string.IsNullOrWhiteSpace(Name);

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (!_db.IsOpen) return;

        await _db.ExecuteQueryAsync("BudgetCategories/Insert.sql", new
        {
            Name,
            Parent            = string.IsNullOrEmpty(SelectedParent) ? null : SelectedParent,
            IsExcludedFromSpendingTotal,
            BudgetTarget
        });

        Clear();
        await LoadOptionsAsync();
    }

    [RelayCommand]
    private void Clear()
    {
        Name = string.Empty;
        SelectedParent = null;
        IsExcludedFromSpendingTotal = false;
        BudgetTarget = null;
    }
}