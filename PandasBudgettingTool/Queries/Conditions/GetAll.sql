SELECT Id, RuleName, Rank, AndOr, IsStringProperty, TransactionProperty, Conditional, Value
FROM Condition
ORDER BY RuleName, Rank, Id;