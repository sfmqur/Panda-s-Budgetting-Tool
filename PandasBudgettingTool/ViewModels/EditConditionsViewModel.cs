using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PandasBudgettingTool.Models;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class EditConditionsViewModel : ViewModelBase
{
    private const string AllRulesFilter = "(All)";

    private readonly DatabaseService _db;
    private List<ConditionRowViewModel> _allConditions = [];

    public EditConditionsViewModel(DatabaseService db) => _db = db;

    public ObservableCollection<ConditionRowViewModel> Conditions { get; } = [];

    public ObservableCollection<string> RuleFilterOptions { get; } = [AllRulesFilter];

    [ObservableProperty]
    private string _selectedRuleFilter = AllRulesFilter;

    partial void OnSelectedRuleFilterChanged(string value) => ApplyFilter();

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

    private void ApplyFilter()
    {
        var filtered = SelectedRuleFilter == AllRulesFilter
            ? _allConditions
            : _allConditions.Where(c => c.RuleName == SelectedRuleFilter);

        Conditions.Clear();
        foreach (var c in filtered)
            Conditions.Add(c);
    }
}