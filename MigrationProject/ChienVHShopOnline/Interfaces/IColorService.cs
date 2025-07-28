using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Interfaces;

public interface IColorService
{
    Task<IEnumerable<ColorReadDto>> GetAllAsync();
    Task<ColorReadDto?> GetByIdAsync(int id);
    Task<ColorReadDto> CreateAsync(ColorCreateDto dto);
    Task<bool> UpdateAsync(int id, ColorUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
