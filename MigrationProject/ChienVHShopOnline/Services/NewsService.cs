using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using ChienVHShopOnline.Models;
using ChienVHShopOnline.Data;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Services;

public class NewsService : INewsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public NewsService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<NewsReadDto>> GetAllAsync()
    {
        var news = await _context.News.Include(n => n.User).ToListAsync();
        return _mapper.Map<IEnumerable<NewsReadDto>>(news);
    }

    public async Task<NewsReadDto?> GetByIdAsync(int id)
    {
        var news = await _context.News.Include(n => n.User).FirstOrDefaultAsync(n => n.NewsId == id);
        return news == null ? null : _mapper.Map<NewsReadDto>(news);
    }

    public async Task<PagedResultDto<NewsReadDto>> GetPagedNewsAsync(int pageNumber, int pageSize)
    {
        var totalItems = await _context.News.CountAsync();

        var news = await _context.News
            .OrderByDescending(n => n.NewsId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<NewsReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResultDto<NewsReadDto>
        {
            Items = news,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }


    public async Task<NewsReadDto> CreateAsync(NewsCreateDto dto)
    {
        var news = _mapper.Map<News>(dto);
        _context.News.Add(news);
        await _context.SaveChangesAsync();
        return _mapper.Map<NewsReadDto>(news);
    }

    public async Task<bool> UpdateAsync(NewsUpdateDto dto)
    {
        var news = await _context.News.FindAsync(dto.NewsId);
        if (news == null) return false;

        _mapper.Map(dto, news);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var news = await _context.News.FindAsync(id);
        if (news == null) return false;

        _context.News.Remove(news);
        await _context.SaveChangesAsync();
        return true;
    }
}
