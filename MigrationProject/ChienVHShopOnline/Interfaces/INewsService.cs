using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Interfaces;

public interface INewsService
{
    Task<IEnumerable<NewsReadDto>> GetAllAsync();
    Task<NewsReadDto?> GetByIdAsync(int id);
    Task<PagedResultDto<NewsReadDto>> GetPagedNewsAsync(int pageNumber, int pageSize);
    Task<NewsReadDto> CreateAsync(NewsCreateDto dto);
    Task<bool> UpdateAsync(NewsUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}