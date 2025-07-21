using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;

namespace TrainingVideoPortal.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _container;
        private readonly ILogger<BlobService> _logger;

        public BlobService(IConfiguration config, ILogger<BlobService> logger)
        {
            var connectionString = config["AzureBlob:ConnectionString"];
            var containerName = config["AzureBlob:ContainerName"];
            _logger = logger;
            _container = new BlobContainerClient(connectionString, containerName);
            _container.CreateIfNotExists();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var blobClient = _container.GetBlobClient(Guid.NewGuid() + Path.GetExtension(file.FileName));

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return GetSasUrl(blobClient);
        }

        private string GetSasUrl(BlobClient blobClient)
        {
            var sas = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));
            return sas.ToString();
        }

        public string GetSasUrlFromBlobUrl(string blobUrl, int expiry = 1)
        {
            var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);
            var blobClient = _container.GetBlobClient(blobName);
            _logger.LogInformation("Generating SAS for blob: {BlobName}", blobName);
            _logger.LogInformation("Blob client URI: {BlobClientUri}", blobClient.Uri);

            if (!blobClient.CanGenerateSasUri)
                throw new InvalidOperationException("SAS URI generation not permitted. Check credentials.");

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _container.Name,
                BlobName = blobName,
                Resource = "b", // blob
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(expiry)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString(); // full link with token
        }

    }
}
