using System;
using AutoMapper;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.EventAssignment;
using UserDepartmentModel = Synergy.Underwriting.Models.UserDepartmentModel;

namespace Synergy.Underwriting.API.Mappings
{
    public class EventMappingProfile : Profile
    {
        public EventMappingProfile()
        {
            this.CreateMap<EventCreateArgs, EventCreateCommand>()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                .ForMember(x => x.CreatedBy, exp => exp.Ignore())
                .ForMember(x => x.UserId, exp => exp.MapFrom(x => x.AssignedToUserId))
                .ForMember(x => x.TypeId, exp => exp.MapFrom(x => (int?)x.Type))
                .ForMember(x => x.AuctionTypeId, exp => exp.MapFrom(x => (int?)x.AuctionType))
                .ForMember(x => x.FinalPaymentTypeId, exp => exp.MapFrom(x => (int?)x.FinalPaymentType))
                .ForMember(x => x.SaleDateStatusId, exp => exp.MapFrom(x => (int?)x.SaleDateStatus))
                .ForMember(x => x.UserDepartments, exp => exp.MapFrom(x => x.UserDepartments));

            this.CreateMap<EventUpdateArgs, EventUpdateCommand>()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                .ForMember(x => x.CreatedBy, exp => exp.Ignore())
                .ForMember(x => x.UserId, exp => exp.MapFrom(x => x.AssignedToUserId))
                .ForMember(x => x.TypeId, exp => exp.MapFrom(x => (int?)x.Type))
                .ForMember(x => x.AuctionTypeId, exp => exp.MapFrom(x => (int?)x.AuctionType))
                .ForMember(x => x.FinalPaymentTypeId, exp => exp.MapFrom(x => (int?)x.FinalPaymentType))
                .ForMember(x => x.SaleDateStatusId, exp => exp.MapFrom(x => (int?)x.SaleDateStatus))
                .ForMember(x => x.UserDepartments, exp => exp.MapFrom(x => x.UserDepartments));
        }
    }
}
