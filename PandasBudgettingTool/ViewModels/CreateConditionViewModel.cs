using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PandasBudgettingTool.Services;

namespace PandasBudgettingTool.ViewModels;

public partial class CreateConditionViewModel : ViewModelBase
{
    private readonly DatabaseService _db;

    public CreateConditionViewModel(DatabaseService db) => _db = db;

    // ── Static option lists ───────────────────────────────────────────────────

    public static string[] TransactionProperties { get; } =
        ["Name", "Amount", "Date", "Description", "AccountName", "BudgetCategoryName"];

    private static readonly string[] StringConditionals  =
        ["Contains", "StartsWith", "EndsWith", "Equals", "NotEquals"];

    private static readonly string[] NumericConditionals =
        ["Equals", "NotEquals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual"];

    public string[] ConditionalOptions => IsStringProperty ? StringConditionals : NumericConditionals;

    // ── DB-loaded options ─────────────────────────────────────────────────────

    public ObservableCollection<string> RuleNames { get; } = [];

    public async Task LoadOptionsAsync()
    {
        if (!_db.IsOpen) return;
        RuleNames.Clear();
        foreach (var n in await _db.QueryAsync<string>("Rules/GetAllNames.sql"))
            RuleNames.Add(n);
    }

    // ── Form properties ───────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string? _selectedRuleName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string? _selectedTransactionProperty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConditionalOptions))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private bool _isStringProperty = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string? _selectedConditional;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _value = string.Empty;

    // When the property type changes, clear the conditional since options change
    partial void OnIsStringPropertyChanged(bool value) => SelectedConditional = null;

    private bool CanCreate() =>
        !string.IsNullOrWhiteSpace(SelectedRuleName)          &&
        !string.IsNullOrWhiteSpace(SelectedTransactionProperty) &&
        !string.IsNullOrWhiteSpace(SelectedConditional)        &&
        !string.IsNullOrWhiteSpace(Value);

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (!_db.IsOpen) return;

        await _db.ExecuteQueryAsync("Conditions/Insert.sql", new
        {
            Id                  = System.Guid.NewGuid().ToString(),
            RuleName            = SelectedRuleName,
            IsStringProperty,
            TransactionProperty = SelectedTransactionProperty,
            Conditional         = SelectedConditional,
            Value
        });

        Clear();
    }

    [RelayCommand]
    private void Clear()
    {
        SelectedRuleName            = null;
        SelectedTransactionProperty = null;
        IsStringProperty            = true;
        SelectedConditional         = null;
        Value                       = string.Empty;
    }
}