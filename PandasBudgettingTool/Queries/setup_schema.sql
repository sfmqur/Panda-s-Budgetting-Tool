-- Panda's Budgeting Tool — initial schema setup
-- Run once on File > New Database to initialize all tables.
-- Sign convention: negative amount = expense, positive = income.

PRAGMA foreign_keys = ON;

-- Tracks the version of each table for future migrations
CREATE TABLE IF NOT EXISTS TableVersions (
    TableName TEXT NOT NULL PRIMARY KEY,
    Version   INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Account (
    Name                  TEXT NOT NULL PRIMARY KEY,
    IsMinusSignAnExpense  INTEGER NOT NULL DEFAULT 1,  -- 1 = true
    ImporterType          TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS BudgetCategory (
    Name                       TEXT NOT NULL PRIMARY KEY,
    Parent                     TEXT NULL REFERENCES BudgetCategory(Name) ON DELETE SET NULL,
    IsExcludedFromSpendingTotal INTEGER NOT NULL DEFAULT 0,
    BudgetTarget               REAL NULL
);

CREATE TABLE IF NOT EXISTS RuleCategory (
    Id                   TEXT NOT NULL PRIMARY KEY,   -- GUID
    Name                 TEXT NOT NULL,
    ParentRuleCategoryId TEXT NULL REFERENCES RuleCategory(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Rule (
    Id                 TEXT NOT NULL PRIMARY KEY,   -- GUID
    Name               TEXT NOT NULL,
    Rank               INTEGER NOT NULL DEFAULT 0,
    BudgetCategoryName TEXT NULL REFERENCES BudgetCategory(Name) ON DELETE SET NULL,
    RuleCategoryId     TEXT NULL REFERENCES RuleCategory(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Condition (
    Id                  TEXT NOT NULL PRIMARY KEY,   -- GUID
    RuleId              TEXT NOT NULL REFERENCES Rule(Id) ON DELETE CASCADE,
    IsStringProperty    INTEGER NOT NULL DEFAULT 1,  -- 1 = string, 0 = numeric
    TransactionProperty TEXT NOT NULL,               -- e.g. "Name", "Amount", "Date"
    Conditional         TEXT NOT NULL,               -- e.g. "Contains", "Equals", "GreaterThan"
    Value               TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS [Transaction] (
    Id                     TEXT NOT NULL PRIMARY KEY,  -- composite: Date|Name|Amount
    Date                   TEXT NOT NULL,
    Name                   TEXT NOT NULL,
    Description            TEXT NOT NULL DEFAULT '',
    Amount                 REAL NOT NULL,
    BudgetCategoryName     TEXT NULL REFERENCES BudgetCategory(Name) ON DELETE SET NULL,
    UserAdjustedCategoryName TEXT NULL REFERENCES BudgetCategory(Name) ON DELETE SET NULL,
    AccountName            TEXT NULL REFERENCES Account(Name) ON DELETE SET NULL
);

-- Seed version rows (update Version when schema changes)
INSERT OR IGNORE INTO TableVersions (TableName, Version) VALUES ('Account',         1);
INSERT OR IGNORE INTO TableVersions (TableName, Version) VALUES ('BudgetCategory',  1);
INSERT OR IGNORE INTO TableVersions (TableName, Version) VALUES ('RuleCategory',    1);
INSERT OR IGNORE INTO TableVersions (TableName, Version) VALUES ('Rule',            1);
INSERT OR IGNORE INTO TableVersions (TableName, Version) VALUES ('Condition',       1);
INSERT OR IGNORE INTO TableVersions (TableName, Version) VALUES ('Transaction',     1);