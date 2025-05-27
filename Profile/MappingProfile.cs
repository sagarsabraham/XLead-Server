using AutoMapper;
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Existing mappings
            CreateMap<CompanyCreateDto, Company>();
            CreateMap<ContactCreateDto, Contact>();
            CreateMap<Company, CompanyReadDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Not Active"));

            CreateMap<Contact, ContactReadDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName));

            CreateMap<Company, CompanyReadDto>()
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Not Active"))
    .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => "Admin User")); // 

            CreateMap<User, UserReadDto>()
    .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
        src.CreatedByUser != null ? src.CreatedByUser.Name : "System"));

            CreateMap<UserCreateDto, User>();


        }
    }
}