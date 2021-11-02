using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class LeadModelMapProfile : Profile
    {
        public LeadModelMapProfile()
        {
            CreateMap<Lead, LeadModel>()
                    .ForMember(e => e.Address, t => t.MapFrom(src => src))
                        ;
            CreateMap<Lead, LeadAddressModel>()
                    .ForMember(e => e.State, t => t.MapFrom(src => src.MailingState))
                    .ForMember(e => e.City, t => t.MapFrom(src => src.MailingCity))
                    .ForMember(e => e.Zip, t => t.MapFrom(src => src.MailingZipCode))
                    .ForMember(e => e.Address1, t => t.MapFrom(src => src.MailingAddress1))
                    .ForMember(e => e.Address2, t => t.MapFrom(src => src.MailingAddress2))
                    .ForMember(e => e.Address3, t => t.MapFrom(src => src.MailingAddress3))
                    ;
        }
    }
}
