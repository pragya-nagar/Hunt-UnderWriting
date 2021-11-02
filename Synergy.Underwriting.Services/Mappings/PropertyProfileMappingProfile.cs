using AutoMapper;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands.PropertyProfile;

namespace Synergy.Underwriting.Services.Mappings
{
    public class PropertyProfileMappingProfile : Profile
    {
        public PropertyProfileMappingProfile()
        {
            this.CreateMap<PropertyProfileCreateCommand, CreatePropertyProfileModel>();

            this.CreateMap<PropertyProfileUpdateCommand, UpdatePropertyProfileModel>();
        }
    }
}
