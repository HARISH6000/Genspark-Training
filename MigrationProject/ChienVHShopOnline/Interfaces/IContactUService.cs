using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Interfaces;

public interface IContactUService
{
    Task<IEnumerable<ContactUReadDto>> GetAllAsync();
    Task<ContactUReadDto?> GetByIdAsync(int id);
    Task<ContactUReadDto> CreateAsync(ContactUCreateDto dto);
    Task<bool> DeleteAsync(int id);
}
