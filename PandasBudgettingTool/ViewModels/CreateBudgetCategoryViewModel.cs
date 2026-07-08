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
    private decimal? _budgetTarget = 0;

    [ObservableProperty]
    private bool _createAssociatedRule = true;

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

        if (CreateAssociatedRule)
        {
            await _db.ExecuteQueryAsync("Rules/Insert.sql", new
            {
                Name,
                Rank               = 0,
                BudgetCategoryName = Name,
                RuleCategoryName   = (string?)null
            });
        }

        Clear();
        await LoadOptionsAsync();
    }

    [RelayCommand]
    private void Clear()
    {
        Name = string.Empty;
        SelectedParent = null;
        IsExcludedFromSpendingTotal = false;
        BudgetTarget = 0;
        CreateAssociatedRule = true;
    }

    public override async Task SaveAsync()
    {
        if (CanCreate()) await Create();
    }

    public override async Task RefreshAsync() => await LoadOptionsAsync();
}