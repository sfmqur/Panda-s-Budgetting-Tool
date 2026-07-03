-- Clear every reference before deleting so the parent row is never left with
-- dangling children under foreign_keys enforcement.
UPDATE RuleCategory SET ParentRuleCategoryName = NULL WHERE ParentRuleCategoryName = @Name;
UPDATE Rule SET RuleCategoryName = NULL WHERE RuleCategoryName = @Name;
DELETE FROM RuleCategory WHERE Name = @Name;