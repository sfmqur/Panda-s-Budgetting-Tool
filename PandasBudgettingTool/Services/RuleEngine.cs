using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PandasBudgettingTool.Models;

namespace PandasBudgettingTool.Services;

public class RuleEngine
{
    private static readonly HashSet<string> AllowedTransactionProperties =
        new(["Name", "Amount", "Date", "Description", "AccountName", "BudgetCategoryName"]);

    private readonly DatabaseService _db;

    public RuleEngine(DatabaseService db) => _db = db;

    /// <summary>
    /// Clears BudgetCategory on every non-user-adjusted Transaction in range (or the whole database when
    /// <paramref name="executeOnAll"/> is true), then re-applies every Rule, lowest Rank first, to assign
    /// BudgetCategory to the Transactions each Rule's Conditions match.
    /// </summary>
    public async Task ExecuteAsync(DateTime fromDate, DateTime toDate, bool executeOnAll = false)
    {
        if (!_db.IsOpen) return;

        var rangeClause = executeOnAll ? "" : " AND Date >= @FromDate AND Date <= @ToDate";

        var clearParams = new DynamicParameters();
        if (!executeOnAll)
        {
            clearParams.Add("FromDate", fromDate.ToString("yyyy-MM-dd"));
            clearParams.Add("ToDate", toDate.ToString("yyyy-MM-dd"));
        }
        
        var numAffected = await _db.ExecuteRawAsync(
            $"UPDATE [Transaction] SET BudgetCategoryName = NULL WHERE UserAdjustedCategory = 0{rangeClause}",
            clearParams);

        var rules = (await _db.QueryAsync<Rule>("Rules/GetAll.sql"))
            .OrderBy(r => r.Rank)
            .ToList();

        var conditionsByRule = (await _db.QueryAsync<Condition>("Conditions/GetAll.sql"))
            .GroupBy(c => c.RuleName)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Rank).ToList());

        foreach (var rule in rules)
        {
            if (!conditionsByRule.TryGetValue(rule.Name, out var conditions) || conditions.Count == 0)
                continue;

            if (conditions.Any(c => !AllowedTransactionProperties.Contains(c.TransactionProperty)))
                continue;

            var parameters = new DynamicParameters();
            parameters.Add("BudgetCategoryName", rule.BudgetCategoryName);
            if (!executeOnAll)
            {
                parameters.Add("FromDate", fromDate.ToString("yyyy-MM-dd"));
                parameters.Add("ToDate", toDate.ToString("yyyy-MM-dd"));
            }

            string whereClause;
            try
            {
                whereClause = BuildRuleWhereClause(conditions, parameters);
            }
            catch (FormatException)
            {
                // A Condition's Value doesn't parse as numeric even though IsStringProperty is false — skip this Rule.
                continue;
            }

            var sql = "UPDATE [Transaction] SET BudgetCategoryName = @BudgetCategoryName " +
                      $"WHERE UserAdjustedCategory = 0{rangeClause} AND ({whereClause})";

            await _db.ExecuteRawAsync(sql, parameters);
        }
    }

    private static string BuildRuleWhereClause(List<Condition> conditions, DynamicParameters parameters)
    {
        var paramIndex = 0;
        var clause = BuildPredicate(conditions[0], parameters, ref paramIndex);

        for (var i = 1; i < conditions.Count; i++)
        {
            var op = string.Equals(conditions[i].AndOr, "And", StringComparison.OrdinalIgnoreCase) ? "AND" : "OR";
            var predicate = BuildPredicate(conditions[i], parameters, ref paramIndex);
            clause = $"({clause} {op} {predicate})";
        }

        return clause;
    }

    private static string BuildPredicate(Condition condition, DynamicParameters parameters, ref int paramIndex)
    {
        var paramName = $"cond{paramIndex++}";
        string sqlOperator;
        object value;

        switch (condition.Conditional)
        {
            case "Contains":
                sqlOperator = "LIKE";
                value = $"%{condition.Value}%";
                break;
            case "StartsWith":
                sqlOperator = "LIKE";
                value = $"{condition.Value}%";
                break;
            case "EndsWith":
                sqlOperator = "LIKE";
                value = $"%{condition.Value}";
                break;
            case "Equals":
                sqlOperator = "=";
                value = condition.IsStringProperty ? condition.Value : decimal.Parse(condition.Value);
                break;
            case "NotEquals":
                sqlOperator = "<>";
                value = condition.IsStringProperty ? condition.Value : decimal.Parse(condition.Value);
                break;
            case "GreaterThan":
                sqlOperator = ">";
                value = decimal.Parse(condition.Value);
                break;
            case "LessThan":
                sqlOperator = "<";
                value = decimal.Parse(condition.Value);
                break;
            case "GreaterThanOrEqual":
                sqlOperator = ">=";
                value = decimal.Parse(condition.Value);
                break;
            case "LessThanOrEqual":
                sqlOperator = "<=";
                value = decimal.Parse(condition.Value);
                break;
            default:
                throw new InvalidOperationException($"Unknown Conditional '{condition.Conditional}'.");
        }

        parameters.Add(paramName, value);
        return $"{condition.TransactionProperty} {sqlOperator} @{paramName}";
    }
}