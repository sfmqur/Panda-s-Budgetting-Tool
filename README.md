# Panda's Budgetting Tool
A tool used for autoimport of account statements in order to analyze spending. 

## Future Features
- Account Statement import into accounts
- On Import, it fires the conditions and rules to sort into budget categories
  - On account Creation, can select an importer Type
  - There is an importer factory to handle the different import formats. 
## Implemented Features



## Data Model
Sign Convention: minus sign is an expense, positive sign is income.

Transaction: (unique ID based on Date-Name-Amount) V1
- Date
- Name (as imported, not editable in UI)
- Description (empty, user editable)
- Amount 
- BudgeCategory
- UserAdjustedCategory (excludes from rerun of rules)
- Account

Account: V1
- Name (primary key) 
- IsMinusSignAnExpense (used on import file)
- ImporterType

BudgetCategory:
- Name (Primary key)
- IsExcludedFromSpendingTotal

Rule: V1
- Name
- Rank
- List of Condition
- BudgetCategory
- RuleCategory

Condition: V1
- GUID
- IsStringProperty
- TransactionProperty
- Conditional
- Value

RuleCategory: V1
- ParentRuleCategory (NULL allowed)
- Name

TableVersions: 
Property of Table name and int for  current version.
Previous table version stored in [[OldModel]].md
one row
If version gets updated, can migrate to new table version and update this record. 
