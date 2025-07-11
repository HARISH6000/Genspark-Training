using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Exceptions;

namespace InventoryManagementAPI.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadBasePath;
        private readonly IAuditLogService _auditLogService; 
        public LocalFileStorageService(string uploadBasePath, IAuditLogService auditLogService)
        {
            _uploadBasePath = uploadBasePath;
            _auditLogService = auditLogService; 
            
            if (!Directory.Exists(_uploadBasePath))
            {
                Directory.CreateDirectory(_uploadBasePath);
            }
        }
        
        public string GetSasUrl(string blobUrl, TimeSpan expiry)
        {
            // Local file storage does not support SAS URLs like Azure Blob Storage.
            // This method can be left unimplemented or throw an exception.
            throw new NotImplementedException("SAS URL generation is not supported for local file storage.");
        }

        public async Task<string> SaveFileAsync(byte[] fileBytes, string fileName, string contentType)
        {

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new UnsupportedMediaTypeException("Unsupported file type. Only image files (jpg, jpeg, png, gif, bmp, tiff) are allowed.");
            }


            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadBasePath, uniqueFileName);

            try
            {
                await File.WriteAllBytesAsync(filePath, fileBytes);

                return uniqueFileName;
            }
            catch (Exception ex)
            {

                throw new Exception($"Could not save file: {ex.Message}", ex);
            }
        }

        public void DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var filePath = Path.Combine(_uploadBasePath, fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine($"Could not delete file {filePath}: {ex.Message}");
                    
                }
            }
        }
    }
}