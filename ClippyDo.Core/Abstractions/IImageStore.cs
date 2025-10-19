namespace ClippyDo.Core.Abstractions;

public interface IImageStore
{
    Task<string> SaveAsync(byte[] imageBytes, string extension, CancellationToken ct = default);
    Task<byte[]?> ReadAsync(string key, CancellationToken ct = default);
}