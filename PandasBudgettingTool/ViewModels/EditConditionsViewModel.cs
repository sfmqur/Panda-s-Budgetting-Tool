using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class EditConditionsViewModel : ViewModelBase
{
    private const string AllRulesFilter = "(All)";

    private readonly DatabaseService _db;
    private readonly DialogService   _dialogService;
    private List<ConditionRowViewModel> _allConditions = [];

    public EditConditionsViewModel(DatabaseService db, DialogService dialogService)
    {
        _db            = db;
        _dialogService = dialogService;
    }

    public ObservableCollection<ConditionRowViewModel> Conditions { get; } = [];

    public ObservableCollection<string> RuleFilterOptions { get; } = [AllRulesFilter];

    [ObservableProperty]
    private string _selectedRuleFilter = AllRulesFilter;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSelectedRuleFilterChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void ApplyFilters() => ApplyFilter();

    public override async Task RefreshAsync()
    {
        if (!_db.IsOpen) return;

        var ruleNames = (await _db.QueryAsync<string>("Rules/GetAllNames.sql")).ToList();

        var conditions = await _db.QueryAsync<Condition>("Conditions/GetAll.sql");
        _allConditions = conditions.Select(c => new ConditionRowViewModel(c, ruleNames)).ToList();

        var prevFilter = SelectedRuleFilter;
        RuleFilterOptions.Clear();
        RuleFilterOptions.Add(AllRulesFilter);
        foreach (var name in ruleNames)
            RuleFilterOptions.Add(name);

        SelectedRuleFilter = RuleFilterOptions.Contains(prevFilter) ? prevFilter : AllRulesFilter;
        ApplyFilter();
    }

    public override async Task SaveAsync()
    {
        if (!_db.IsOpen) return;

        foreach (var row in _allConditions)
            await _db.ExecuteQueryAsync("Conditions/Update.sql", row.ToUpdateParam());
    }

    /// <summary>Confirms with the user, then deletes the given condition.</summary>
    public async Task DeleteConditionAsync(ConditionRowViewModel row)
    {
        if (!_db.IsOpen) return;

        var confirmDelete = await _dialogService.ConfirmAsync(
            "Delete Condition",
            $"Are you sure you want to delete this condition ({row.TransactionProperty} {row.Conditional} \"{row.Value}\")?");
        if (!confirmDelete) return;

        await _db.ExecuteQueryAsync("Conditions/Delete.sql", new { row.Id });
        _allConditions.Remove(row);
        Conditions.Remove(row);
    }

    private void ApplyFilter()
    {
        var searchText = SearchText?.Trim() ?? string.Empty;

        var filtered = _allConditions.Where(c =>
            (SelectedRuleFilter == AllRulesFilter || c.RuleName == SelectedRuleFilter) &&
            (searchText.Length == 0 || c.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase)));

        Conditions.Clear();
        foreach (var c in filtered)
            Conditions.Add(c);
    }
}