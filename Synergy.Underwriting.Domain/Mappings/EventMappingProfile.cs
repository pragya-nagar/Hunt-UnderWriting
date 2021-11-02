using System;
using AutoMapper;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.EventAssignment;
using UserDepartmentModel = Synergy.Underwriting.Models.UserDepartmentModel;

namespace Synergy.Underwriting.Domain.Mappings
{
    public class EventMappingProfile : Profile
    {
        public EventMappingProfile()
        {
            this.CreateMap<Underwriting.Models.Commands.Event.UserDepartmentModel, UserDepartmentModel>()
                .ReverseMap();

            this.CreateMap<UserDepartmentModel, DAL.Queries.Original.Models.UserDepartmentModel>()
                .ReverseMap();

            this.CreateMap<DAL.Queries.Original.Models.EventModel, EventModel>()
                .ForMember(x => x.County, exp => exp.MapFrom(x => x.County))
                .ForMember(x => x.Type, exp => exp.MapFrom(x => x.EventType))
                .ForMember(x => x.AssignedTo, exp => exp.Ignore())
                .ForMember(x => x.Number, exp => exp.MapFrom(x => x.EventNumber))
                .ForMember(x => x.DueDate, exp => exp.Ignore());

            this.CreateMap<DAL.Queries.Original.Models.EventModel, EventDetailsModel>()
                .ForMember(x => x.UserDepartments, exp => exp.MapFrom(x => x.UserDepartments))
                .ForMember(x => x.AssignedTo, exp => exp.Ignore())
                .ForMember(x => x.County, exp => exp.MapFrom(x => x.County))
                .ForMember(x => x.Type, exp => exp.MapFrom(x => x.EventType))
                .ForMember(x => x.Number, exp => exp.MapFrom(x => x.EventNumber))
                .ForMember(x => x.OriginalListCount, exp => exp.MapFrom(x => x.OriginalListCount))
                .ForMember(x => x.OriginalListAmount, exp => exp.MapFrom(x => x.OriginalListAmount));

            this.CreateMap<DAL.Queries.Original.Models.Assigment.LevelModel, AssignmentLevelModel>()
                .ForMember(x => x.Assignment, exp => exp.Ignore())
                .ForMember(x => x.AvailableRecords, exp => exp.Ignore());

            this.CreateMap<Guid, SetEventLockStatusCommand>()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                .ForMember(x => x.CreatedBy, exp => exp.Ignore())
                .ForMember(x => x.EventId, dest => dest.MapFrom(x => x));

            this.CreateMap<DAL.Queries.Original.Models.EventCalculatedFieldsModel, EventCalculatedFieldsModel>()
               .ForMember(x => x.Id, dest => dest.MapFrom(x => x.EventId));

            this.CreateMap<DAL.Queries.Original.Models.Assigment.EventAssignmentModel, EventAssignmentResult>();
            this.CreateMap<DAL.Queries.Original.Models.Assigment.EventAssignmentLevelModel, AssignmentLevelModel>();
        }
    }
}
