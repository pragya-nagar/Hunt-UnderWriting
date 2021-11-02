using AutoMapper;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Bid;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Domain.Mappings
{
    public class BidMappingProfile : Profile
    {
        public BidMappingProfile()
        {
            this.CreateMap<DAL.Queries.Entities.Bid, BidModel>();

            this.CreateMap<DAL.Queries.Entities.Bid, BidDetailsModel>();
        }
    }
}
