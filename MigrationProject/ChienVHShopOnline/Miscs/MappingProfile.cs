using AutoMapper;
using ChienVHShopOnline.Models;
using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Color, ColorReadDto>();
        CreateMap<ColorCreateDto, Color>();
        CreateMap<ColorUpdateDto, Color>();

        CreateMap<Category, CategoryReadDto>();
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();

        CreateMap<News, NewsReadDto>();
        CreateMap<NewsCreateDto, News>();
        CreateMap<NewsUpdateDto, News>();
    }
}
