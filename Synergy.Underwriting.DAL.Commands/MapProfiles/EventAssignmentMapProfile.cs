using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class EventAssignmentMapProfile : Profile
    {
        public EventAssignmentMapProfile()
        {
            CreateMap<LevelAssignmentModel, EventDecisionLevel>()
                .IgnoreAuditMembers()
                .ForMember(x => x.Id, t => t.MapFrom(src => src.LevelId))
                .ForMember(x => x.EventId, t => t.Ignore())
                .ForMember(x => x.EventDecisionLevelUser, t => t.Ignore())
                .ForMember(x => x.Event, t => t.Ignore())
                .ForMember(x => x.Decisions, t => t.Ignore());

            CreateMap<PropertyProfileLevelAssignmentModel, EventDecisionLevelPropertyProfile>()
                .IgnoreAuditMembers()
                .ForMember(x => x.EventDecisionLevelId, t => t.Ignore())
                .ForMember(x => x.Id, t => t.Ignore())
                .ForMember(x => x.Order, t => t.MapFrom(src => src.ProfileOrder))
                .ForMember(x => x.EventId, t => t.Ignore())
                .ForMember(x => x.EventDecisionLevel, t => t.Ignore())
                .ForMember(x => x.Event, t => t.Ignore())
                .ForMember(x => x.PropertyProfile, t => t.Ignore());

            CreateMap<UserAssignmentModel, EventDecisionLevelUser>()
               .IgnoreAuditMembers()
               .ForMember(x => x.EventDecisionLevelPropertyProfileId, t => t.Ignore())
               .ForMember(x => x.EventDecisionLevelId, t => t.Ignore())
               .ForMember(x => x.Id, t => t.Ignore())
               .ForMember(x => x.EventDecisionLevel, t => t.Ignore())
               .ForMember(x => x.EventDecisionLevelPropertyProfile, t => t.Ignore())
               .ForMember(x => x.User, t => t.Ignore())
               .ForMember(x => x.AssigmentCount, t => t.MapFrom(src => src.AssignmentsCount));
        }
    }
}
