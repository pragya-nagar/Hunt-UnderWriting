using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateDataCutRuleModelMapProfile : Profile
    {
        public CreateDataCutRuleModelMapProfile()
        {
            CreateMap<CreateDataCutRuleModel, DataCutRule>()
                .IgnoreAuditMembers()
                .ForMember(r => r.DataCutResultType, src => src.Ignore())
                .ForMember(r => r.County, src => src.Ignore())
                .ForMember(r => r.DataCutRuleItems, src => src.MapFrom(r => r.DataCutRuleItems))
                ;

            CreateMap<DataCutRuleItemModel, DataCutRuleItem>()
                .IgnoreAuditMembers()
                .ForMember(r => r.DataCutRuleId, src => src.Ignore())
                .ForMember(r => r.DataCutRule, src => src.Ignore())
                .ForMember(r => r.DataCutLogicType, src => src.Ignore())
                .ForMember(r => r.DataCutRuleField, src => src.Ignore())
                ;
        }
    }
}
