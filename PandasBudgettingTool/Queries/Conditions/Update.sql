UPDATE Condition
SET RuleName            = @RuleName,
    IsStringProperty    = @IsStringProperty,
    TransactionProperty = @TransactionProperty,
    Conditional         = @Conditional,
    Value               = @Value
WHERE Id = @Id;