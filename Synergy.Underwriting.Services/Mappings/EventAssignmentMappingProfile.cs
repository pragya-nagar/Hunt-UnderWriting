using AutoMapper;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands.EventAssignment;

namespace Synergy.Underwriting.Services.Mappings
{
    public class EventAssignmentMappingProfile : Profile
    {
        public EventAssignmentMappingProfile()
        {
            CreateMap<EventAssignmentCreateCommand, EventAssignmentModel>();
            CreateMap<EventAssignmentUpdateCommand, EventAssignmentModel>();
            CreateMap<Models.PropertyProfile.LevelAssignmentModel, LevelAssignmentModel>();
            CreateMap<Models.PropertyProfile.PropertyProfileLevelAssignmentModel, PropertyProfileLevelAssignmentModel>();
            CreateMap<Models.PropertyProfile.UserAssignmentsModel, UserAssignmentModel>();
        }
    }
}
