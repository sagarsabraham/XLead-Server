using AutoMapper;
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Profiles
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Privilege, PrivilegeReadDto>();

            CreateMap<CustomerCreateDto, Customer>();
            CreateMap<ContactCreateDto, Contact>();

           
            CreateMap<Customer, CustomerReadDto>()
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Not Active"));

            
            CreateMap<Contact, ContactReadDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.customer.CustomerName));


        }

    }
}
