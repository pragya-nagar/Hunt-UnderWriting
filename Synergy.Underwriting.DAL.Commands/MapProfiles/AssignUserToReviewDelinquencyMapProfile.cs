using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class AssignUserToReviewDelinquencyMapProfile : Profile
    {
        public AssignUserToReviewDelinquencyMapProfile()
        {
            CreateMap<AssignUserToReviewDelinquencyModel, Decision>()
                .IgnoreAuditMembers()
                .ForMember(c => c.Comment, src => src.Ignore())
                .ForMember(u => u.User, src => src.Ignore())
                .ForMember(u => u.UserId, src => src.MapFrom(d => d.UserId))
                .ForMember(e => e.EventDecisionLevel, src => src.Ignore())
                .ForMember(e => e.EventDecisionLevelId, src => src.MapFrom(d => d.EventDecisionLevelId))
                .ForMember(dt => dt.DecisionType, src => src.Ignore())
                .ForMember(dt => dt.DecisionTypeId, src => src.Ignore())
                .ForMember(ed => ed.Delinquency, src => src.Ignore())
                .ForMember(ed => ed.DecisionDate, src => src.Ignore())
                .ForMember(ed => ed.PropertyProfile, src => src.Ignore())
                .ForMember(ed => ed.PropertyProfileId, src => src.Ignore())
                ;
        }
    }
}
