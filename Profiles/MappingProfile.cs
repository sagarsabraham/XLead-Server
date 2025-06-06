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
            CreateMap<ContactUpdateDto, Contact>()
           .ForMember(dest => dest.Id, opt => opt.Ignore()) // Good practice: never map over the primary key
           .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // THE FIX: Tell AutoMapper to not touch the CustomerId
           .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Good practice: ignore audit fields that shouldn't change on update
           .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<CustomerUpdateDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());




            CreateMap<CustomerCreateDto, Customer>()
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                 .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                 .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Customer, CustomerReadDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"));
            CreateMap<ContactCreateDto, Contact>()
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Contact, ContactReadDto>();
            CreateMap<DealCreateDto, Deal>()
                 .ForMember(dest => dest.DealName, opt => opt.MapFrom(src => src.Title))
                 .ForMember(dest => dest.DealAmount, opt => opt.MapFrom(src => src.Amount))
                 .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId ?? 0))
                 .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.RegionId))
                 .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.DomainId ?? 0))
                 .ForMember(dest => dest.DealStageId, opt => opt.MapFrom(src => src.DealStageId))
                 .ForMember(dest => dest.RevenueTypeId, opt => opt.MapFrom(src => src.RevenueTypeId))
                 .ForMember(dest => dest.DuId, opt => opt.MapFrom(src => src.DuId))
                 .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
                 .ForMember(dest => dest.ServiceLineId, opt => opt.MapFrom(src => src.ServiceId ?? 0))
                 .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                 .ForMember(dest => dest.Probability, opt => opt.MapFrom(src => src.Probability ?? 0))
                 .ForMember(dest => dest.StartingDate, opt => opt.MapFrom(src => src.StartingDate))
                 .ForMember(dest => dest.ClosingDate, opt => opt.MapFrom(src => src.ClosingDate))
                 .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                 .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Map Deal to DealReadDto
            CreateMap<Deal, DealReadDto>()
                .ForMember(dest => dest.DealName, opt => opt.MapFrom(src => src.DealName))
                .ForMember(dest => dest.DealAmount, opt => opt.MapFrom(src => src.DealAmount))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.account != null ? src.account.AccountName : null))
                .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.RegionId))
                .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.region != null ? src.region.RegionName : null))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.DomainId))
                .ForMember(dest => dest.DomainName, opt => opt.MapFrom(src => src.domain != null ? src.domain.DomainName : null))
                .ForMember(dest => dest.RevenueTypeId, opt => opt.MapFrom(src => src.RevenueTypeId))
                .ForMember(dest => dest.RevenueTypeName, opt => opt.MapFrom(src => src.revenueType != null ? src.revenueType.RevenueTypeName : null))
                .ForMember(dest => dest.DuId, opt => opt.MapFrom(src => src.DuId))
                .ForMember(dest => dest.DUName, opt => opt.MapFrom(src => src.du != null ? src.du.DUName : null))
                .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.country != null ? src.country.CountryName : null))
                .ForMember(dest => dest.DealStageId, opt => opt.MapFrom(src => src.DealStageId))
                .ForMember(dest => dest.StageName, opt => opt.MapFrom(src => src.dealStage != null ? src.dealStage.StageName : null))
                .ForMember(dest => dest.ContactId, opt => opt.MapFrom(src => src.ContactId))
                .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.contact != null ? $"{src.contact.FirstName} {src.contact.LastName}".Trim() : null))
                .ForMember(dest => dest.StartingDate, opt => opt.MapFrom(src => src.StartingDate))
                .ForMember(dest => dest.ClosingDate, opt => opt.MapFrom(src => src.ClosingDate))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
           

        }


    }

    }

