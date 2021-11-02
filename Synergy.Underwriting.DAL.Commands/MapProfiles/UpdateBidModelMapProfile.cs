using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class UpdateBidModelMapProfile : Profile
    {
        public UpdateBidModelMapProfile()
        {
            this.CreateMap<UpdateBidModel, Bid>()
                .IncludeBase<CreateBidModel, Bid>();
        }
    }
}
