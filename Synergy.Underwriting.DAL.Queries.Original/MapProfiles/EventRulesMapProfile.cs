using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class EventRulesMapProfile : Profile
    {
        public EventRulesMapProfile()
        {
            CreateMap<EventDataCutRule, EventRulesModel>()
                .ForMember(x => x.DataCutResultType, t => t.MapFrom(x => x.DataCutRule.DataCutResultType))
                .ForMember(x => x.DataCutRuleItems, t => t.MapFrom(x => x.DataCutRule.DataCutRuleItems))
                .ForMember(x => x.RuleId, t => t.MapFrom(x => x.DataCutRule.Id))
                .ForMember(x => x.EventId, t => t.MapFrom(x => x.EventDataCutStrategy.EventId));
        }
    }
}
