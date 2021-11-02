using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class UpdateEventModelMapProfile : Profile
    {
        public UpdateEventModelMapProfile()
        {
            CreateMap<UpdateEventModel, Event>()
                .IgnoreAuditMembers()
                .ForMember(e => e.User, t => t.Ignore())
                .ForMember(e => e.Bids, t => t.Ignore())
                .ForMember(e => e.EventAttachments, t => t.Ignore())
                .ForMember(e => e.FinalPaymentType, t => t.Ignore())
                .ForMember(e => e.EventUsers, t => t.Ignore())
                .ForMember(e => e.State, t => t.Ignore())
                .ForMember(e => e.EventType, t => t.Ignore())
                .ForMember(e => e.AuctionType, t => t.Ignore())
                .ForMember(e => e.SaleDateStatus, t => t.Ignore())
                .ForMember(e => e.EventEntity, t => t.Ignore())
                .ForMember(e => e.EventDecisionLevels, t => t.Ignore())
                .ForMember(e => e.EventDataCutStrategies, t => t.Ignore())
                .ForMember(e => e.Delinquencies, t => t.Ignore())
                .ForMember(e => e.OriginalListAmount, t => t.Ignore())
                .ForMember(e => e.OriginalListCount, t => t.Ignore())
                .ForMember(e => e.County, t => t.Ignore())
                .ForMember(e => e.EventUsers, t => t.Ignore())
                .ForMember(e => e.IsFreezed, t => t.Ignore())
                ;
        }
    }
}
