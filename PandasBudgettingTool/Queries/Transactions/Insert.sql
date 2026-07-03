-- OR IGNORE skips rows whose Id (Date|Name|Amount) already exists, implementing
-- duplicate detection on statement import without failing the whole batch.
INSERT OR IGNORE INTO [Transaction] (Id, Date, Name, Description, Amount, BudgetCategoryName, UserAdjustedCategory, AccountName)
VALUES (@Id, @Date, @Name, @Description, @Amount, @BudgetCategoryName, @UserAdjustedCategory, @AccountName);