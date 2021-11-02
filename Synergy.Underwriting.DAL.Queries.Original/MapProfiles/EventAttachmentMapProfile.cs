using AutoMapper;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class EventAttachmentMapProfile : Profile
    {
        public EventAttachmentMapProfile()
        {
            CreateMap<EventAttachment, EventAttachmentModel>()
              .ForMember(eam => eam.Data, src => src.Ignore())
              .ApplyAuditMembers();
        }
    }
}
