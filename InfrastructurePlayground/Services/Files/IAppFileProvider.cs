namespace Services.Files;

public interface IAppFileProvider
{
    Task Initialise();
    Task SaveFileAsync(Stream fileStream, string originalName, string contentType, CancellationToken ct);
    Task DeleteFileAsync(string key, CancellationToken ct);
    Task<string> GetFileUrlAsync(string key, CancellationToken ct);
    Task<string> GetUploadUrlAsync(string key, string contentType, CancellationToken ct);
}