using AutoMapper;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Entities;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class DataCutResultTypeMapProfile : Profile
    {
        public DataCutResultTypeMapProfile()
        {
            CreateMap<DataCutResultType, FastEntityModel<int>>()
                .ForMember(f => f.Name, src => src.MapFrom(r => r.Description));
        }
    }
}
