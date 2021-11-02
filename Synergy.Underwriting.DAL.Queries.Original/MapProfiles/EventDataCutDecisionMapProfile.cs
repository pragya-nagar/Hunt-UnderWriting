using AutoMapper;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class EventDataCutDecisionMapProfile : Profile
    {
        public EventDataCutDecisionMapProfile()
        {
            CreateMap<EventDataCutDecision, EventDataCutDecisionModel>()
                .ForMember(ed => ed.EventId, m => m.MapFrom(ed => ed.EventDataCutStrategy.EventId));

            CreateMap<EventDataCutDecision, FastEntityModel<int>>()
                .ForMember(fe => fe.Id, m => m.MapFrom(ed => ed.DecisionType.Id))
                .ForMember(fe => fe.Name, m => m.MapFrom(ed => ed.DecisionType.Description));
        }
    }
}
