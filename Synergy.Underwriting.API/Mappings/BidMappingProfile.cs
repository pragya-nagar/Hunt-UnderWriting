using AutoMapper;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Bid;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.API.Mappings
{
    public class BidMappingProfile : Profile
    {
        public BidMappingProfile()
        {
            this.CreateMap<BidCreateArgs, BidCreateCommand>()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                .ForMember(x => x.CreatedBy, exp => exp.Ignore());

            this.CreateMap<BidUpdateArgs, BidUpdateCommand>()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                .ForMember(x => x.CreatedBy, exp => exp.Ignore());
        }
    }
}
