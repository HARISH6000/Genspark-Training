using FirstAPI.Models.DTOs;

namespace FirstAPI.Interfaces
{
    public interface IFileProcessingService
    {
        public Task<FileUploadReturnDTO> ProcessData(CsvUploadDto csvUploadDto);
    }
}