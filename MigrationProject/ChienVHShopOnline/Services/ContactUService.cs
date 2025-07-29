using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChienVHShopOnline.Data;
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Services;

public class ContactUService : IContactUService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ContactUService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ContactUReadDto>> GetAllAsync()
    {
        return await _context.ContactUs
            .ProjectTo<ContactUReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ContactUReadDto?> GetByIdAsync(int id)
    {
        var contact = await _context.ContactUs.FindAsync(id);
        return contact == null ? null : _mapper.Map<ContactUReadDto>(contact);
    }

    public async Task<ContactUReadDto> CreateAsync(ContactUCreateDto dto)
    {
        var contact = _mapper.Map<ContactU>(dto);
        _context.ContactUs.Add(contact);
        await _context.SaveChangesAsync();
        return _mapper.Map<ContactUReadDto>(contact);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contact = await _context.ContactUs.FindAsync(id);
        if (contact == null) return false;
        _context.ContactUs.Remove(contact);
        await _context.SaveChangesAsync();
        return true;
    }
}
