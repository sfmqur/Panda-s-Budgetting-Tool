UPDATE Rule
SET Rank               = @Rank,
    BudgetCategoryName = @BudgetCategoryName,
    RuleCategoryName    = @RuleCategoryName
WHERE Name = @Name;