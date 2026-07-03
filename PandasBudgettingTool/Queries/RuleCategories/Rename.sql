-- Renaming a RuleCategory requires repointing referencing child categories
-- and rules in the same transaction; defer_foreign_keys lets all three
-- UPDATEs happen before FK constraints are checked at COMMIT.
PRAGMA defer_foreign_keys = ON;
BEGIN;
UPDATE RuleCategory SET Name = @NewName WHERE Name = @OldName;
UPDATE RuleCategory SET ParentRuleCategoryName = @NewName WHERE ParentRuleCategoryName = @OldName;
UPDATE Rule SET RuleCategoryName = @NewName WHERE RuleCategoryName = @OldName;
COMMIT;