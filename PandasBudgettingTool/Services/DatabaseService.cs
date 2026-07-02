using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace PandasBudgettingTool.Services;

public class DatabaseService : IDisposable
{
    private SqliteConnection? _connection;

    public string? CurrentPath { get; private set; }
    public bool IsOpen => _connection is not null;

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new SQLite database at <paramref name="path"/>, runs setup_schema.sql,
    /// and opens the connection.
    /// </summary>
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

    /// <summary>
    /// Opens an existing SQLite database at <paramref name="path"/>.
    /// </summary>
    public async Task OpenAsync(string path)
    {
        Close();

        _connection = BuildConnection(path);
        await _connection.OpenAsync();
        CurrentPath = path;
    }

    /// <summary>
    /// Returns the open connection for use with Dapper.
    /// Throws if no database is open.
    /// </summary>
    public SqliteConnection GetConnection()
    {
        if (_connection is null)
            throw new InvalidOperationException("No database is open.");
        return _connection;
    }

    public void Close()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        // Release any pooled file handles so the file can be moved/deleted
        SqliteConnection.ClearAllPools();
        CurrentPath = null;
    }

    public void Dispose() => Close();

    // ── Private helpers ──────────────────────────────────────────────────────

    private static SqliteConnection BuildConnection(string path)
    {
        // Foreign Keys=True enforces FK constraints at the driver level
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
}