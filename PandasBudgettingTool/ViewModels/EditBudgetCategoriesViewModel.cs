using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class EditBudgetCategoriesViewModel : ViewModelBase
{
    private const string AllParentsFilter = "(All)";
    private const string NoParentFilter   = "(No Parent)";

    private readonly DatabaseService _db;
    private readonly DialogService   _dialogService;
    private List<BudgetCategoryRowViewModel> _allCategories = [];

    public EditBudgetCategoriesViewModel(DatabaseService db, DialogService dialogService)
    {
        _db            = db;
        _dialogService = dialogService;
    }

    public ObservableCollection<BudgetCategoryRowViewModel> Categories { get; } = [];

    public ObservableCollection<string> ParentFilterOptions { get; } = [AllParentsFilter];

    [ObservableProperty]
    private string _selectedParentFilter = AllParentsFilter;

    partial void OnSelectedParentFilterChanged(string value) => ApplyFilter();

    public override async Task RefreshAsync()
    {
        if (!_db.IsOpen) return;

        var categories = (await _db.QueryAsync<BudgetCategory>("BudgetCategories/GetAll.sql")).ToList();
        var allNames = new List<string?> { string.Empty };
        allNames.AddRange(categories.Select(c => c.Name));

        _allCategories = categories
            .Select(c => new BudgetCategoryRowViewModel(c, allNames.Where(n => n != c.Name).ToList()))
            .ToList();

        var prevFilter = SelectedParentFilter;
        ParentFilterOptions.Clear();
        ParentFilterOptions.Add(AllParentsFilter);
        ParentFilterOptions.Add(NoParentFilter);
        foreach (var name in categories.Select(c => c.Name).OrderBy(n => n))
            ParentFilterOptions.Add(name);

        SelectedParentFilter = ParentFilterOptions.Contains(prevFilter) ? prevFilter : AllParentsFilter;
        ApplyFilter();
    }

    public override async Task SaveAsync()
    {
        if (!_db.IsOpen) return;

        foreach (var row in _allCategories)
        {
            if (!string.IsNullOrWhiteSpace(row.Name) && row.Name != row.OriginalName)
            {
                await _db.ExecuteQueryAsync("BudgetCategories/Rename.sql", new { OldName = row.OriginalName, NewName = row.Name });
                row.MarkRenamed();
            }

            await _db.ExecuteQueryAsync("BudgetCategories/Update.sql", row.ToUpdateParam());
        }
    }

    /// <summary>Confirms with the user, then deletes the category, clearing it from any Rules/Transactions that reference it.</summary>
    public async Task DeleteBudgetCategoryAsync(BudgetCategoryRowViewModel row)
    {
        if (!_db.IsOpen) return;

        var confirmDelete = await _dialogService.ConfirmAsync(
            "Delete Budget Category",
            $"Are you sure you want to delete the budget category \"{row.Name}\"?\n\n" +
            "Any Rules or Transactions referencing it will have their Budget Category cleared.");
        if (!confirmDelete) return;

        await _db.ExecuteQueryAsync("BudgetCategories/Delete.sql", new { row.Name });

        _allCategories.Remove(row);
        Categories.Remove(row);
    }

    private void ApplyFilter()
    {
        IEnumerable<BudgetCategoryRowViewModel> filtered = SelectedParentFilter switch
        {
            AllParentsFilter => _allCategories,
            NoParentFilter   => _allCategories.Where(c => string.IsNullOrEmpty(c.Parent)),
            _                => _allCategories.Where(c => c.Parent == SelectedParentFilter)
        };

        Categories.Clear();
        foreach (var c in filtered)
            Categories.Add(c);
    }
}