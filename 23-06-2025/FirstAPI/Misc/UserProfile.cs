using AutoMapper;
using FirstAPI.Models;
using FirstAPI.Models.DTOs.DoctorSpecialities;

namespace FirstAPI.Misc
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<DoctorAddRequestDto, User>()
            .ForMember(dest => dest.Username, act => act.MapFrom(src => src.Email))
            .ForMember(dest => dest.Password, opt => opt.Ignore());
           
            CreateMap<User, DoctorAddRequestDto>()
            .ForMember(dest => dest.Email, act => act.MapFrom(src => src.Username));
   
        }
    }
}