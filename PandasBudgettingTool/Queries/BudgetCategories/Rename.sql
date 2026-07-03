-- Renaming a BudgetCategory requires repointing referencing child categories,
-- Rules, and Transactions in the same transaction; defer_foreign_keys lets
-- all UPDATEs happen before FK constraints are checked at COMMIT.
PRAGMA defer_foreign_keys = ON;
BEGIN;
UPDATE BudgetCategory SET Name = @NewName WHERE Name = @OldName;
UPDATE BudgetCategory SET Parent = @NewName WHERE Parent = @OldName;
UPDATE Rule SET BudgetCategoryName = @NewName WHERE BudgetCategoryName = @OldName;
UPDATE [Transaction] SET BudgetCategoryName = @NewName WHERE BudgetCategoryName = @OldName;
COMMIT;