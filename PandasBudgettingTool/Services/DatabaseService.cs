using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace PandasBudgettingTool.Services;

public class DatabaseService : IDisposable
{
    private SqliteConnection? _connection;

    public string? CurrentPath { get; private set; }
    public bool IsOpen => _connection is not null;

    // ── Open / create ────────────────────────────────────────────────────────

    public async Task CreateNewAsync(string path)
    {
        Close();

        if (File.Exists(path))
            File.Delete(path);

        _connection = BuildConnection(path);
        await _connection.OpenAsync();
        CurrentPath = path;

        await ExecuteSchemaAsync();
    }

    public async Task OpenAsync(string path)
    {
        Close();

        _connection = BuildConnection(path);
        await _connection.OpenAsync();
        CurrentPath = path;
    }

    public void Close()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        SqliteConnection.ClearAllPools();
        CurrentPath = null;
    }

    public void Dispose() => Close();

    // ── Dapper helpers ───────────────────────────────────────────────────────

    /// <summary>Loads a .sql file from Queries/<paramref name="queryFile"/> and returns mapped rows.</summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(string queryFile, object? param = null)
    {
        var sql = await LoadSqlAsync(queryFile);
        return await GetConnection().QueryAsync<T>(sql, param);
    }

    /// <summary>Executes a dynamically-built SQL string directly (e.g. for queries with IN clauses).</summary>
    public async Task<IEnumerable<T>> QueryRawAsync<T>(string sql, object? param = null)
    {
        return await GetConnection().QueryAsync<T>(sql, param);
    }

    /// <summary>Loads a .sql file from Queries/<paramref name="queryFile"/>, executes it, and returns the affected row count.</summary>
    public async Task<int> ExecuteQueryAsync(string queryFile, object? param = null)
    {
        var sql = await LoadSqlAsync(queryFile);
        return await GetConnection().ExecuteAsync(sql, param);
    }

    /// <summary>Returns the open connection for callers that need direct Dapper access.</summary>
    public SqliteConnection GetConnection()
    {
        if (_connection is null)
            throw new InvalidOperationException("No database is open.");
        return _connection;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static SqliteConnection BuildConnection(string path)
    {
        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            ForeignKeys = true,
        }.ToString();

        return new SqliteConnection(cs);
    }

    private async Task ExecuteSchemaAsync()
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Queries", "setup_schema.sql");
        var sql = await File.ReadAllTextAsync(schemaPath);

        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    private static Task<string> LoadSqlAsync(string queryFile)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "Queries", queryFile);
        return File.ReadAllTextAsync(fullPath);
    }
}