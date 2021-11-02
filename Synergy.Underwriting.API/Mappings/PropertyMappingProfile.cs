using System.Linq;
using AutoMapper;
using Synergy.Underwriting.DAL.Queries.Original.Models;
using Synergy.Underwriting.Models.Address;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Property;

namespace Synergy.Underwriting.API.Mappings
{
    public class PropertyMappingProfile : Profile
    {
        public PropertyMappingProfile()
        {
            this.CreateMap<PropertyUpdateArgs, PropertyUpdateCommand>()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                .ForMember(x => x.CreatedBy, exp => exp.Ignore());

            this.CreateMap<MakeDecisionArgs, MakeDecisionCommand>()
                .ForMember(x => x.Id, exp => exp.Ignore())
                .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                .ForMember(x => x.CreatedBy, exp => exp.Ignore());
        }
    }
}
