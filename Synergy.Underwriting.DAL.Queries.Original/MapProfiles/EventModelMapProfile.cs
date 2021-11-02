using System;
using System.Linq;
using AutoMapper;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class EventModelMapProfile : Profile
    {
        public EventModelMapProfile()
        {
            this.CreateMap<EventAttachment, FastEntityModel<Guid>>()
                .ForMember(e => e.Id, t => t.MapFrom(src => src.Id))
                .ForMember(e => e.Name, t => t.MapFrom(src => src.FileName));

            this.CreateMap<Event, EventModel>()
                .ForMember(e => e.Attachments, t => t.MapFrom(src => src.EventAttachments))
                .ForMember(e => e.EventType, t => t.MapFrom(src => src.EventTypeId))
                .ForMember(e => e.AuctionType, t => t.MapFrom(src => src.AuctionTypeId))
                .ForMember(e => e.SaleDateStatus, t => t.MapFrom(src => src.SaleDateStatusId))
                .ForMember(e => e.EventEntity, t => t.MapFrom(src => src.EventEntityId))
                .ForMember(e => e.FinalPaymentType, t => t.MapFrom(src => src.FinalPaymentTypeId))
                .ForMember(e => e.IsAssigned, t => t.MapFrom(src => src.EventDecisionLevels.Any(x => x.DeletedOn == null)))
                .ForMember(e => e.UserDepartments, t => t.MapFrom(src => src.EventUsers))

                // Add mapping to data cut strategy
                .ForMember(e => e.EventDataCutRules, t => t.Ignore())
                .ApplyAuditMembers()
                ;

            this.CreateMap<EventUser, UserDepartmentModel>()
                .ForMember(e => e.DepartmentId, t => t.MapFrom(src => src.DepartmentId))
                .ForMember(e => e.UserId, t => t.MapFrom(src => src.UserId));
        }
    }
}