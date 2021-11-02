using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class EventPropertyResultMapProfile : Profile
    {
        public EventPropertyResultMapProfile()
        {
            CreateMap<Result, EventPropertyResultModel>()
                ;

            CreateMap<Bid, EventPropertyBidModel>()
                 .ForMember(e => e.Portfolio, t => t.MapFrom(src => src.Portfolio))
                 .ForMember(e => e.PurchasingEntity, t => t.MapFrom(src => src.Entity))
                ;
        }
    }
}
