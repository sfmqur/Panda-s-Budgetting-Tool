using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class CreateRuleCategoryViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public CreateRuleCategoryViewModel(DatabaseService db) => _db = db;

    public ObservableCollection<string> ExistingCategories { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _selectedParentRuleCategoryName;

    public async Task LoadOptionsAsync()
    {
        if (!_db.IsOpen) return;
        ExistingCategories.Clear();
        foreach (var n in await _db.QueryAsync<string>("RuleCategories/GetAllNames.sql"))
            ExistingCategories.Add(n);
    }

    private bool CanCreate() => !string.IsNullOrWhiteSpace(Name);

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (!_db.IsOpen) return;

        await _db.ExecuteQueryAsync("RuleCategories/Insert.sql", new
        {
            Name,
            ParentRuleCategoryName = string.IsNullOrEmpty(SelectedParentRuleCategoryName)
                ? null : SelectedParentRuleCategoryName
        });

        Clear();
        await LoadOptionsAsync();
    }

    [RelayCommand]
    private void Clear()
    {
        Name = string.Empty;
        SelectedParentRuleCategoryName = null;
    }
}