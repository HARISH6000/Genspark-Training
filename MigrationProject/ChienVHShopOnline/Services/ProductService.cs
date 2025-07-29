using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using ChienVHShopOnline.Models;
using ChienVHShopOnline.Data;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryId)
    {
        var query = _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.Color)
            .Include(p => p.Model)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        return await query
            .OrderByDescending(p => p.ProductId)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.Color)
            .Include(p => p.Model)
            .Where(p => p.ProductId == id)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> UpdateProductAsync(ProductUpdateDto dto)
    {
        var existingProduct = await _context.Products.FindAsync(dto.ProductId);
        if (existingProduct == null) return false;

        _mapper.Map(dto, existingProduct);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

}
