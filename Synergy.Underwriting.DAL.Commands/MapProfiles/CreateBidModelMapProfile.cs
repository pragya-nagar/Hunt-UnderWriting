using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateBidModelMapProfile : Profile
    {
        public CreateBidModelMapProfile()
        {
            this.CreateMap<CreateBidModel, Bid>()
                .IgnoreAuditMembers()
                .ForMember(e => e.Event, t => t.Ignore());
        }
    }
}
