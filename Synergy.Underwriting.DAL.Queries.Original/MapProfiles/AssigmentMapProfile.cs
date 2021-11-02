using AutoMapper;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models.Assigment;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class AssigmentMapProfile : Profile
    {
        public AssigmentMapProfile()
        {
            CreateMap<EventDecisionLevel, LevelModel>()

             // .ForMember(e => e.Decisions, t => t.MapFrom(src => src.Decisions))
             .ApplyAuditMembers()
                ;

            CreateMap<Decision, DecisionModel>()

                // TODO check query
                   .ForMember(e => e.DelinquencyId, t => t.MapFrom(src => src.DelinquencyId))
                   .ForMember(e => e.Value, t => t.MapFrom(src => src.DecisionTypeId))
                   .ApplyAuditMembers()
                ;
        }
    }
}
