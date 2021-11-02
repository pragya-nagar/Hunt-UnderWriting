using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateResultModelMapProfile : Profile
    {
        public CreateResultModelMapProfile()
        {
            this.CreateMap<CreateResultModel, Result>()
                .IgnoreAuditMembers()
                .ForMember(e => e.Delinquency, t => t.Ignore())
                .ForMember(e => e.BidId, t => t.Ignore())
                .ForMember(e => e.Bid, t => t.Ignore());
        }
    }
}
