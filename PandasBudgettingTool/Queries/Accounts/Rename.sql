-- Renaming an Account requires repointing referencing Transactions in the
-- same transaction; defer_foreign_keys lets both UPDATEs happen before the
-- FK constraint on Transaction.AccountName is checked at COMMIT.
PRAGMA defer_foreign_keys = ON;
BEGIN;
UPDATE Account SET Name = @NewName WHERE Name = @OldName;
UPDATE [Transaction] SET AccountName = @NewName WHERE AccountName = @OldName;
COMMIT;