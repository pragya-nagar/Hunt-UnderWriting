using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Attachment;
using Synergy.Underwriting.Models.Commands.Event;

namespace Synergy.Underwriting.Services.Mappings
{
    public class EventCommandsMappingProfile : Profile
    {
        public EventCommandsMappingProfile()
        {
            this.CreateMap<UserDepartmentModel, DAL.Commands.Models.UserDepartmentsModel>()
                .ReverseMap();

            this.CreateMap<Synergy.Underwriting.Models.UserDepartmentModel, DAL.Commands.Models.UserDepartmentsModel>()
                .ReverseMap();

            this.CreateMap<EventCreateCommand, DAL.Commands.Models.CreateEventModel>()
                .ForMember(x => x.CountyId, exp => exp.MapFrom(x => x.CountyId))
                .ForMember(x => x.CountyName, exp => exp.MapFrom(x => x.CountyName))
                .ForMember(x => x.EventTypeId, exp => exp.MapFrom(x => x.TypeId))
                .ForMember(x => x.EventEntityId, exp => exp.Ignore())
                .ForMember(x => x.EventNumber, exp => exp.Ignore())
                .ForMember(x => x.UserDepartments, exp => exp.MapFrom(x => x.UserDepartments));

            this.CreateMap<EventUpdateCommand, DAL.Commands.Models.UpdateEventModel>()
                .ForMember(x => x.CountyId, exp => exp.MapFrom(x => x.CountyId))
                .ForMember(x => x.EventTypeId, exp => exp.MapFrom(x => x.TypeId))
                .ForMember(x => x.EventEntityId, exp => exp.Ignore())
                .ForMember(x => x.EventNumber, exp => exp.Ignore())
                .ForMember(x => x.UserDepartments, exp => exp.MapFrom(x => x.UserDepartments));

            this.CreateMap<AttachmentCreateCommand, Underwriting.DAL.Commands.Models.AttachFileToEventModel>()
                .ForMember(x => x.Id, exp => exp.MapFrom(x => x.Id));

            this.CreateMap<KeyValuePair<Guid, IEnumerable<Guid>>, DAL.Commands.Models.EventDecisionLevelUserModel>()
                .ForMember(x => x.UserId, exp => exp.MapFrom(x => x.Key))
                .ForMember(x => x.AssigmentCount, exp => exp.MapFrom(x => x.Value.Count()));

            this.CreateMap<EventAttachmentDeleteCommand, Underwriting.DAL.Commands.Models.DeleteAttachmentModel>();
        }
    }
}
