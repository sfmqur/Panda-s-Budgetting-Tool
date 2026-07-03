# Panda's Budgeting Tool

A desktop budgeting tool that auto-imports account statements, categorizes transactions via rules, and analyzes spending against budget targets.

## Tech Stack

- **.NET 10 / Avalonia 12** — cross-platform desktop UI
- **CommunityToolkit.Mvvm** — MVVM source generators (`[ObservableProperty]`, `[RelayCommand]`)
- **Dapper + Microsoft.Data.Sqlite** — lightweight SQLite access via `.sql` query files in `Queries/`
- **ViewLocator** — auto-resolves `*ViewModel` → `*View` for page navigation

## Project Goals

1. **Importable account statements** — parse CSV/OFX exports from banks into a unified Transaction list.
2. **Rule-based auto-categorization** — on import, evaluate ranked rules (each with N conditions) to assign a BudgetCategory to each transaction.
3. **Budget tracking** — define categories with optional targets and view actual vs. budgeted spending.
4. **Spending analysis** — aggregate transactions by category across date ranges; support hierarchical budget categories.
5. **Extensible importers** — importer factory pattern so new bank formats can be added without changing core logic.

## Roadmap / Tasks

### Phase 1 — Foundation (current)
- [x] Project scaffold: Avalonia + MVVM + Dapper/SQLite
- [x] MainWindow with menu bar (File / Edit / Navigate / Import) and toolbar
- [x] Page-based navigation shell (Transactions, Accounts, Budget, Spending, Rules)
- [x] SQLite schema (`Queries/setup_schema.sql`)
- [ ] `FileNewCommand` — create empty SQLite DB and run setup_schema.sql
- [ ] `FileOpenCommand` — open existing `.db` file via dialog, set connection string
- [ ] `FileSaveCommand` — flush / close current connection cleanly

### Phase 2 — Accounts & Import
- [ ] Accounts page: list, add, edit, delete accounts
- [ ] Importer factory + CSV importer (column-mapped, configurable sign convention)
- [ ] Import Statement dialog: choose account, choose file, preview rows, confirm import
- [ ] Duplicate detection on import (hash: Date|Name|Amount)

### Phase 3 — Rules Engine
- [ ] Rules page: create/edit/delete rules with ranked ordering
- [ ] Condition editor: TransactionProperty + Conditional + Value (string or numeric)
- [ ] Rule runner: on import and on-demand, evaluate rules in rank order and assign BudgetCategory
- [ ] `UserAdjustedCategory` flag — prevents rule re-run from overwriting manual overrides

### Phase 4 — Transactions & Budget
- [ ] Transactions page: filterable/sortable grid with inline category editing
- [ ] Budget page: category tree with BudgetTarget vs. actual spend columns
- [ ] Spending page: date-range spending summary by category hierarchy

## Data Model

Sign convention: **negative amount = expense, positive = income**.

### Transaction (V1)
| Column | Type    | Notes |
|---|---------|---|
| Id | TEXT PK | Composite key: `Date\|Name\|Amount` |
| Date | TEXT    | ISO-8601 YYYY-MM-DD|
| Name | TEXT    | As imported; user-editable |
| Description | TEXT    | User-editable notes |
| Amount | REAL    | Negative = expense |
| BudgetCategoryName | TEXT FK | Assigned by rules |
| UserAdjustedCategory | INTEGER | 1 = userAdjusted (skip rule re-run), 0 = automatic |
| AccountName | TEXT FK | |

### Account (V1)
| Column | Type | Notes |
|---|---|---|
| Name | TEXT PK | |
| IsMinusSignAnExpense | INTEGER | Import sign convention |
| ImporterType | TEXT | Key into importer factory |

### BudgetCategory (V1)
| Column | Type | Notes |
|---|---|---|
| Name | TEXT PK | |
| Parent | TEXT FK NULL | Self-referential hierarchy |
| IsExcludedFromSpendingTotal | INTEGER | e.g. transfers |
| BudgetTarget | REAL NULL | Monthly target |

### Rule (V1)
One Rule has many Conditions (1:N). All Conditions must match for the Rule to fire.

| Column | Type | Notes |
|---|---|---|
| Name | TEXT PK | |
| Rank | INTEGER | Lower = higher priority |
| BudgetCategoryName | TEXT FK NULL | Category to assign on match |
| RuleCategoryName | TEXT FK NULL | Logical grouping |
| *(Conditions)* | *1:N nav* | Loaded from Condition table via RuleName FK |

### Condition (V1)
| Column | Type | Notes |
|---|---|---|
| Id | TEXT PK | GUID (`Guid.NewGuid().ToString()`) |
| RuleName | TEXT FK | Parent Rule.Name — cascades on delete |
| IsStringProperty | INTEGER | 1 = string comparison, 0 = numeric |
| TransactionProperty | TEXT | `Name`, `Amount`, `Date`, etc. |
| Conditional | TEXT | `Contains`, `Equals`, `GreaterThan`, etc. |
| Value | TEXT | Comparison value (always stored as text) |

### RuleCategory (V1)
| Column | Type         | Notes |
|---|--------------|---|
| Name | TEXT PK      | |
| ParentRuleCategoryName | TEXT FK NULL | Self-referential |

### TableVersions
One row per table (`TableName`, `Version`). Increment `Version` when the schema changes and run a migration.

## Previous Model Notes

See `OldModel.md` for superseded schema versions.