using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class CreateRuleViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public CreateRuleViewModel(DatabaseService db) => _db = db;

    public ObservableCollection<string> BudgetCategoryNames { get; } = [];
    public ObservableCollection<string> RuleCategoryNames   { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal? _rank = 0;

    [ObservableProperty]
    private string? _selectedBudgetCategoryName;

    [ObservableProperty]
    private string? _selectedRuleCategoryName;

    public async Task LoadOptionsAsync()
    {
        if (!_db.IsOpen) return;

        BudgetCategoryNames.Clear();
        foreach (var n in await _db.QueryAsync<string>("BudgetCategories/GetAllNames.sql"))
            BudgetCategoryNames.Add(n);

        RuleCategoryNames.Clear();
        foreach (var n in await _db.QueryAsync<string>("RuleCategories/GetAllNames.sql"))
            RuleCategoryNames.Add(n);
    }

    private bool CanCreate() => !string.IsNullOrWhiteSpace(Name);

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (!_db.IsOpen) return;

        await _db.ExecuteQueryAsync("Rules/Insert.sql", new
        {
            Name,
            Rank               = (int)(Rank ?? 0),
            BudgetCategoryName = string.IsNullOrEmpty(SelectedBudgetCategoryName) ? null : SelectedBudgetCategoryName,
            RuleCategoryName   = string.IsNullOrEmpty(SelectedRuleCategoryName)   ? null : SelectedRuleCategoryName
        });

        Clear();
    }

    [RelayCommand]
    private void Clear()
    {
        Name = string.Empty;
        Rank = 0;
        SelectedBudgetCategoryName = null;
        SelectedRuleCategoryName   = null;
    }
}