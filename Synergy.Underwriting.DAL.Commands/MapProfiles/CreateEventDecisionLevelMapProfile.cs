using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateEventDecisionLevelMapProfile : Profile
    {
        public CreateEventDecisionLevelMapProfile()
        {
            CreateMap<CreateEventDecisionLevelModel, EventDecisionLevel>()
                .IgnoreAuditMembers()
                .ForMember(e => e.Event, src => src.Ignore())
                .ForMember(e => e.EventDecisionLevelUser, src => src.Ignore())
                .ForMember(e => e.EventId, src => src.MapFrom(e => e.EventId))
                .ForMember(e => e.Decisions, src => src.Ignore())
                ;

            CreateMap<EventDecisionLevelUserModel, EventDecisionLevelUser>()
                .IgnoreAuditMembers()
                .ForMember(e => e.Id, src => src.Ignore())
                .ForMember(u => u.User, src => src.Ignore())
                .ForMember(u => u.UserId, src => src.MapFrom(e => e.UserId))
                .ForMember(u => u.EventDecisionLevel, src => src.Ignore())
                .ForMember(u => u.EventDecisionLevelId, src => src.Ignore())
                .ForMember(u => u.EventDecisionLevelPropertyProfile, src => src.Ignore())
                .ForMember(u => u.EventDecisionLevelPropertyProfileId, src => src.Ignore())
                ;
        }
    }
}
