using AutoMapper;
using ChienVHShopOnline.Data;
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Services;

public class ModelService : IModelService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ModelService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ModelDto>> GetAllAsync()
    {
        var models = await _context.Models.ToListAsync();
        return _mapper.Map<IEnumerable<ModelDto>>(models);
    }

    public async Task<ModelDto?> GetByIdAsync(int id)
    {
        var model = await _context.Models.FindAsync(id);
        return model == null ? null : _mapper.Map<ModelDto>(model);
    }

    public async Task<ModelDto> CreateAsync(ModelCreateDto dto)
    {
        var model = _mapper.Map<Model>(dto);
        _context.Models.Add(model);
        await _context.SaveChangesAsync();
        return _mapper.Map<ModelDto>(model);
    }

    public async Task<bool> UpdateAsync(int id, ModelUpdateDto dto)
    {
        var model = await _context.Models.FindAsync(id);
        if (model == null) return false;

        _mapper.Map(dto, model);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var model = await _context.Models.FindAsync(id);
        if (model == null) return false;

        _context.Models.Remove(model);
        await _context.SaveChangesAsync();
        return true;
    }
}
