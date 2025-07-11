namespace InventoryManagementAPI.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(byte[] fileBytes, string fileName, string contentType);
        void DeleteFile(string filePath);

        string GetSasUrl(string blobUrl, TimeSpan expiry);
    }
}