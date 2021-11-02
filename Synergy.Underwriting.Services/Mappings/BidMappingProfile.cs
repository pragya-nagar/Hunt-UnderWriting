using AutoMapper;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services.Mappings
{
    public class BidMappingProfile : Profile
    {
        public BidMappingProfile()
        {
            this.CreateMap<BidCreateCommand, CreateBidModel>();

            this.CreateMap<BidUpdateCommand, UpdateBidModel>();
        }
    }
}
