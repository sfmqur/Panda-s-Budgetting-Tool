UPDATE Condition
SET RuleName            = @RuleName,
    Rank                = @Rank,
    AndOr               = @AndOr,
    IsStringProperty    = @IsStringProperty,
    TransactionProperty = @TransactionProperty,
    Conditional         = @Conditional,
    Value               = @Value
WHERE Id = @Id;