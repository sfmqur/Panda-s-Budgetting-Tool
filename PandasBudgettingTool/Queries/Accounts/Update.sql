UPDATE Account
SET IsMinusSignAnExpense = @IsMinusSignAnExpense,
    ImporterType         = @ImporterType
WHERE Name = @Name;