
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public class BlobStorageService
{
    private const string ContainerName = "event-banners";
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly BlobContainerClient _container;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _container = blobServiceClient.GetBlobContainerClient(ContainerName);
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Please select an image.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("The image must not exceed 5 MB.");

        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Only JPEG, PNG and WebP images are allowed.");

        var extension = file.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ArgumentException("Unsupported image format.")
        };

        var blobName = $"{Guid.NewGuid()}{extension}";
        var blob = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();

        await blob.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            });

        return blobName;
    }

    public async Task<BlobDownloadStreamingResult?> DownloadAsync(string blobName)
    {
        var blob = _container.GetBlobClient(blobName);

        if (!await blob.ExistsAsync())
            return null;

        var response = await blob.DownloadStreamingAsync();

        return response.Value;
    }

    public async Task DeleteAsync(string blobName)
    {
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync();
    }
}
