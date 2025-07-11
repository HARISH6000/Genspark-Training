using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Exceptions;

namespace InventoryManagementAPI.Services
{
    public class BlobFileStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly IAuditLogService _auditLogService;

        public BlobFileStorageService(string connectionString, string containerName, IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container exists
            _containerClient.CreateIfNotExists(PublicAccessType.None); // private container
        }

        public string GetSasUrl(string blobUrl, TimeSpan expiry)
        {
            var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
                throw new InvalidOperationException("SAS URI generation not permitted. Check credentials.");

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b", // blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString(); // full link with token
        }


        public async Task<string> SaveFileAsync(byte[] fileBytes, string fileName, string contentType)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new UnsupportedMediaTypeException("Unsupported file type.");

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            try
            {
                var blobClient = _containerClient.GetBlobClient(uniqueFileName);
                using var stream = new MemoryStream(fileBytes);

                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

                return blobClient.Uri.ToString(); // return full URL
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload to blob storage: {ex.Message}", ex);
            }
        }

        public void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                var blobName = Path.GetFileName(new Uri(fileUrl).LocalPath);
                var blobClient = _containerClient.GetBlobClient(blobName);
                blobClient.DeleteIfExists();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not delete blob: {ex.Message}");
            }
        }
    }
}
