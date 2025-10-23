using System.IO;
using ClippyDo.Core.Abstractions;
using Microsoft.Data.Sqlite;

namespace ClippyDo.Adapter.Sqlite;

internal sealed class SqliteMigrationsTask : IStartupTask
{
    private readonly SqliteOptions _opts;

    public SqliteMigrationsTask(SqliteOptions opts) { _opts = opts; }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var dbPath = ExpandPath(_opts.DatabasePath);
        EnsureDirectory(dbPath);

        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        using var c = new SqliteConnection(cs);
        await c.OpenAsync(ct);

        var cmd = c.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Clips (
            Id TEXT PRIMARY KEY,
            Kind INTEGER NOT NULL,
            PlainText TEXT NULL,
            Rtf BLOB NULL,
            Html BLOB NULL,
            ImageBytes BLOB NULL,
            ImageFormat TEXT NULL,
            ThumbnailBytes BLOB NULL,
            SourceName TEXT NULL,
            SourceProcess TEXT NULL,
            SourceWindowTitle TEXT NULL,
            CreatedAtUtc TEXT NOT NULL,
            LastUsedAtUtc TEXT NOT NULL,
            UsageCount INTEGER NOT NULL,
            IsPinned INTEGER NOT NULL,
            ContentHash TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_Clips_LastUsed ON Clips(LastUsedAtUtc DESC);
        CREATE INDEX IF NOT EXISTS IX_Clips_ContentHash ON Clips(ContentHash);";
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static string ExpandPath(string path)
        => Path.GetFullPath(Environment.ExpandEnvironmentVariables(path).Replace('/', Path.DirectorySeparatorChar));

    private static void EnsureDirectory(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
    }
}
