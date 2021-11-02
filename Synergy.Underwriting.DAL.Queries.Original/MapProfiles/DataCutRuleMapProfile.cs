using AutoMapper;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class DataCutRuleMapProfile : Profile
    {
        public DataCutRuleMapProfile()
        {
            CreateMap<DataCutRule, DataCutRuleModel>()
                .ApplyAuditMembers()
                .ForMember(r => r.DataCutRuleItems, src => src.MapFrom(r => r.DataCutRuleItems));

            CreateMap<DataCutRuleItem, DataCutRuleItemModel>()
                .ForMember(l => l.DataCutLogicType, src => src.MapFrom(l => new FastEntityModel<int>
                {
                    Id = l.DataCutLogicType.Id,
                    Name = l.DataCutLogicType.Description,
                }))
                .ForMember(f => f.DataCutRuleField, src => src.MapFrom(f => new FastEntityModel<int>
                {
                    Id = f.DataCutRuleField.Id,
                    Name = f.DataCutRuleField.Description,
                }));
        }
    }
}
