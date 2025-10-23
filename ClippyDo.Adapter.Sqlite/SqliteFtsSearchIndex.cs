
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;
using Microsoft.Data.Sqlite;

namespace ClippyDo.Adapter.Sqlite;

internal sealed class SqliteFtsSearchIndex : ISearchIndex
{
    private readonly SqliteOptions _opts;
    public SqliteFtsSearchIndex(SqliteOptions opts) { _opts = opts; }

    private SqliteConnection Open()
    {
        var dbPath = System.IO.Path.GetFullPath(
            Environment.ExpandEnvironmentVariables(_opts.DatabasePath)
                .Replace('/', System.IO.Path.DirectorySeparatorChar));

        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        var c = new SqliteConnection(cs);
        c.Open();
        return c;
    }


    public async IAsyncEnumerable<Clip> SearchAsync(string query, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT * FROM Clips WHERE PlainText LIKE $q ORDER BY IsPinned DESC, LastUsedAtUtc DESC LIMIT 200";
        cmd.Parameters.AddWithValue("$q", "%" + query + "%");

        using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            yield return new Clip
            {
                Id = new ClipId(r.GetString(r.GetOrdinal("Id"))),
                Kind = (ClipKind)r.GetInt32(r.GetOrdinal("Kind")),
                PlainText = r["PlainText"] as string,
                Rtf = r["Rtf"] as byte[],
                Html = r["Html"] as byte[],
                ImageBytes = r["ImageBytes"] as byte[],
                ImageFormat = r["ImageFormat"] as string,
                ThumbnailBytes = r["ThumbnailBytes"] as byte[],
                Source = new SourceApp(
                    r["SourceName"] as string ?? "Unknown",
                    r["SourceProcess"] as string ?? "Unknown",
                    r["SourceWindowTitle"] as string
                ),
                ContentHash = new ContentHash(r.GetString(r.GetOrdinal("ContentHash")))
            };
            // Do not assign UsageCount / LastUsedAtUtc / IsPinned (read-only in Clip).
        }
    }
}
