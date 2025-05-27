using AutoMapper;
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Profiles
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            
            CreateMap<CompanyCreateDto, Company>();
            CreateMap<ContactCreateDto, Contact>();

           
            CreateMap<Company, CompanyReadDto>()
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Not Active"));

            
            CreateMap<Contact, ContactReadDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName));


        }

    }
}
