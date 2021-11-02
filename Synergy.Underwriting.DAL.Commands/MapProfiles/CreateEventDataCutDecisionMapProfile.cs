using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateEventDataCutDecisionMapProfile : Profile
    {
        public CreateEventDataCutDecisionMapProfile()
        {
            CreateMap<CreateEventDataCutDecisionModel, EventDataCutDecision>()
                .IgnoreAuditMembers()
                .ForMember(s => s.EventDataCutStrategy, d => d.Ignore())
                .ForMember(s => s.Delinquency, d => d.Ignore())
                .ForMember(s => s.DecisionType, d => d.Ignore())
                ;
        }
    }
}
