
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;
using Microsoft.Data.Sqlite;

namespace ClippyDo.Adapter.Sqlite;

internal sealed class SqliteClipRepository : IClipRepository
{
    private readonly SqliteOptions _opts;

    public SqliteClipRepository(SqliteOptions opts)
    {
        _opts = opts;
    }

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

    public async Task UpsertAsync(Clip clip, CancellationToken ct = default)
    {
        using var c = Open();
        using var tx = c.BeginTransaction();
        // dedupe by ContentHash
        var select = c.CreateCommand();
        select.CommandText = "SELECT Id, UsageCount FROM Clips WHERE ContentHash = $hash";
        select.Parameters.AddWithValue("$hash", clip.ContentHash.Value);
        var existing = await select.ExecuteReaderAsync(ct);
        if (await existing.ReadAsync(ct))
        {
            var id = existing.GetString(0);
            var usage = existing.GetInt32(1) + 1;
            var update = c.CreateCommand();
            update.CommandText = "UPDATE Clips SET LastUsedAtUtc=$lu, UsageCount=$uc WHERE Id=$id";
            update.Parameters.AddWithValue("$lu", DateTime.UtcNow);
            update.Parameters.AddWithValue("$uc", usage);
            update.Parameters.AddWithValue("$id", id);
            await update.ExecuteNonQueryAsync(ct);
            tx.Commit();
            return;
        }

        var cmd = c.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Clips
            (Id, Kind, PlainText, Rtf, Html, ImageBytes, ImageFormat, ThumbnailBytes, SourceName, SourceProcess, SourceWindowTitle, CreatedAtUtc, LastUsedAtUtc, UsageCount, IsPinned, ContentHash)
            VALUES ($id,$kind,$text,$rtf,$html,$img,$iformat,$thumb,$sname,$sproc,$stitle,$cat,$lu,$uc,$pin,$hash)";
        cmd.Parameters.AddWithValue("$id", clip.Id.Value);
        cmd.Parameters.AddWithValue("$kind", (int)clip.Kind);
        cmd.Parameters.AddWithValue("$text", (object?)clip.PlainText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$rtf", (object?)clip.Rtf ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$html", (object?)clip.Html ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$img", (object?)clip.ImageBytes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$iformat", (object?)clip.ImageFormat ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$thumb", (object?)clip.ThumbnailBytes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$sname", (object?)clip.Source?.Name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$sproc", (object?)clip.Source?.Process ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$stitle", (object?)clip.Source?.WindowTitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$cat", clip.CreatedAtUtc);
        cmd.Parameters.AddWithValue("$lu", clip.LastUsedAtUtc);
        cmd.Parameters.AddWithValue("$uc", clip.UsageCount);
        cmd.Parameters.AddWithValue("$pin", clip.IsPinned ? 1 : 0);
        cmd.Parameters.AddWithValue("$hash", clip.ContentHash.Value);
        await cmd.ExecuteNonQueryAsync(ct);
        tx.Commit();
    }

    public async Task<Clip?> GetByIdAsync(ClipId id, CancellationToken ct = default)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Clips WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id.Value);
        using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;
        return Map(r);
    }

    public async IAsyncEnumerable<Clip> GetRecentAsync(int take, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Clips ORDER BY IsPinned DESC, LastUsedAtUtc DESC LIMIT $t";
        cmd.Parameters.AddWithValue("$t", take);
        using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            yield return Map(r);
        }
    }

    public async Task PinAsync(ClipId id, bool isPinned, CancellationToken ct = default)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Clips SET IsPinned=$p WHERE Id=$id";
        cmd.Parameters.AddWithValue("$p", isPinned ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static Clip Map(SqliteDataReader r)
    {
        var kind = (ClipKind)r.GetInt32(r.GetOrdinal("Kind"));

        // Only set fields that have public setters/init; avoid read-only props like UsageCount, LastUsedAtUtc, IsPinned
        var clip = new Clip
        {
            Id = new ClipId(r.GetString(r.GetOrdinal("Id"))),
            Kind = kind,
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

        // NOTE: we intentionally do not set UsageCount / LastUsedAtUtc / IsPinned

        return clip;
    }
}
