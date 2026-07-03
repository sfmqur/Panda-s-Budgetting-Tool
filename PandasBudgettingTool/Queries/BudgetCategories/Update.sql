UPDATE BudgetCategory
SET Parent                      = @Parent,
    IsExcludedFromSpendingTotal = @IsExcludedFromSpendingTotal,
    BudgetTarget                = @BudgetTarget
WHERE Name = @Name;