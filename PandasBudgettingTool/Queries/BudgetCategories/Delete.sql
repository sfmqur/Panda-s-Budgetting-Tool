-- Clear every reference before deleting so the parent row is never left with
-- dangling children under foreign_keys enforcement.
UPDATE Rule SET BudgetCategoryName = NULL WHERE BudgetCategoryName = @Name;
UPDATE [Transaction] SET BudgetCategoryName = NULL WHERE BudgetCategoryName = @Name;
UPDATE BudgetCategory SET Parent = NULL WHERE Parent = @Name;
DELETE FROM BudgetCategory WHERE Name = @Name;