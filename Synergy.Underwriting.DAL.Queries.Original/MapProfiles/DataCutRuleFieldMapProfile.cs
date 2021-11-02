using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class DataCutRuleFieldMapProfile : Profile
    {
        public DataCutRuleFieldMapProfile()
        {
            CreateMap<DataCutRuleField, DataCutRuleFieldModel>()
                .ForMember(e => e.Id, t => t.MapFrom(src => src.Id))
                .ForMember(e => e.Name, t => t.MapFrom(src => src.Description))
                .ForMember(e => e.LogicTypes, t => t.MapFrom(src => src.DataCutFieldType.DataCutLogicTypes))
                ;
        }
    }
}
