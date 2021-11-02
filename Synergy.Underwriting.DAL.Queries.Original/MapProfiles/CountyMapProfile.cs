using AutoMapper;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Entities;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class CountyMapProfile : Profile
    {
        public CountyMapProfile()
        {
            this.CreateMap<County, FastEntityModel<int>>()
                .ForMember(e => e.Id, t => t.MapFrom(src => src.Id))
                .ForMember(e => e.Name, t => t.MapFrom(src => src.Name))
                ;
        }
    }
}
