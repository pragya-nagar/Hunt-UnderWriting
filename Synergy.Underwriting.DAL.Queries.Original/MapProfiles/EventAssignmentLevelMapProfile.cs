using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models.Assigment;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class EventAssignmentLevelMapProfile : Profile
    {
        public EventAssignmentLevelMapProfile()
        {
            CreateMap<EventDecisionLevel, EventAssignmentLevelModel>()
                .ForMember(e => e.AvailableRecords, t => t.Ignore())
                .ForMember(e => e.Assignment, t => t.Ignore())
                ;
        }
    }
}
