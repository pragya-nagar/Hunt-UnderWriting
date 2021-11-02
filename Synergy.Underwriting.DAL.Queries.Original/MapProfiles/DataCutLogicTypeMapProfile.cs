using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class DataCutLogicTypeMapProfile : Profile
    {
        public DataCutLogicTypeMapProfile()
        {
            CreateMap<DataCutLogicType, DataCutLogicTypeModel>()
                .ForMember(e => e.Name, t => t.MapFrom(src => src.Description))
                .ForMember(e => e.FieldDataType, t => t.MapFrom(src => src.DataCutFieldType.Name))
                ;
        }
    }
}
