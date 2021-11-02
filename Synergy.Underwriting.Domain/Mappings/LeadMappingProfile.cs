using AutoMapper;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Address;

namespace Synergy.Underwriting.Domain.Mappings
{
    public class LeadMappingProfile : Profile
    {
        public LeadMappingProfile()
        {
            this.CreateMap<DAL.Queries.Original.Models.LeadAddressModel, AddressModel>();

            this.CreateMap<DAL.Queries.Original.Models.LeadModel, LeadModel>()
                .ForMember(x => x.Name, exp => exp.MapFrom(x => x.AccountName))
                .ForMember(x => x.Address, exp => exp.MapFrom(x => x.Address));
        }
    }
}
