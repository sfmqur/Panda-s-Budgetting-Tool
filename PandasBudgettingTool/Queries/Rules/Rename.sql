-- Renaming a Rule requires repointing referencing Conditions in the same
-- transaction; defer_foreign_keys lets both UPDATEs happen before the FK
-- constraint on Condition.RuleName is checked at COMMIT.
PRAGMA defer_foreign_keys = ON;
BEGIN;
UPDATE Rule SET Name = @NewName WHERE Name = @OldName;
UPDATE Condition SET RuleName = @NewName WHERE RuleName = @OldName;
COMMIT;