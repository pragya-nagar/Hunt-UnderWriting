using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class AttachFileToEventMapProfile : Profile
    {
        public AttachFileToEventMapProfile()
        {
            CreateMap<AttachFileToEventModel, EventAttachment>()
                .IgnoreAuditMembers()
                .ForMember(ea => ea.Event, src => src.Ignore())
                .ForMember(ea => ea.EventId, src => src.MapFrom(e => e.EventId))
                ;
        }
    }
}
