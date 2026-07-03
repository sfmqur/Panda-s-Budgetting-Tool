UPDATE [Transaction]
SET Date                   = @Date,
    Name                   = @Name,
    Description            = @Description,
    Amount                 = @Amount,
    BudgetCategoryName     = @BudgetCategoryName,
    UserAdjustedCategory   = @UserAdjustedCategory,
    AccountName            = @AccountName
WHERE Id = @Id;