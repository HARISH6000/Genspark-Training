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

        CreateMap<ContactU, ContactUReadDto>();
        CreateMap<ContactUCreateDto, ContactU>();

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color != null ? src.Color.Name : null))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Model != null ? src.Model.ModelName : null));
        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();

        CreateMap<Product, CartItemDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

        CreateMap<Order, OrderReadDto>()
            .ForMember(dest => dest.TotalAmount,
            opt => opt.MapFrom(src =>
                src.OrderDetails.Sum(od => (od.Price ?? 0) * (od.Quantity ?? 0))
            ));

        CreateMap<Model, ModelDto>().ReverseMap();
        CreateMap<ModelCreateDto, Model>();
        CreateMap<ModelUpdateDto, Model>();

        CreateMap<User, UserReadDto>();

    }
}
