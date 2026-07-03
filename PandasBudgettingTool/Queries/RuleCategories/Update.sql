UPDATE RuleCategory
SET ParentRuleCategoryName = @ParentRuleCategoryName
WHERE Name = @Name;