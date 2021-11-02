using System;
using AutoMapper;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands.PropertyProfile;
using Synergy.Underwriting.Models.PropertyProfile;
using PropertyProfileRuleItemModel = Synergy.Underwriting.Models.PropertyProfile.PropertyProfileRuleItemModel;
using PropertyProfileRuleItemValueModel = Synergy.Underwriting.Models.Commands.PropertyProfile.PropertyProfileRuleItemValueModel;

namespace Synergy.Underwriting.API.Mappings
{
    public class PropertyProfileMappingProfile : Profile
    {
        public PropertyProfileMappingProfile()
        {
            this.CreateMap<PropertyProfileRuleItemArgs, Underwriting.Models.Commands.PropertyProfile.PropertyProfileRuleItemModel>()
                .ForMember(x => x.PropertyProfileLogicTypeId, exp => exp.MapFrom(x => x.PropertyProfileLogicTypeId))
                .ForMember(x => x.PropertyProfileRuleFieldId, exp => exp.MapFrom(x => x.PropertyProfileRuleFieldId))
                .ForMember(x => x.PropertyProfileRuleItemValues, exp => exp.MapFrom(x => x.PropertyProfileRuleItemValues));

            this.CreateMap<PropertyProfileRuleArgs, PropertyProfileRuleCreateCommand>()
                 .ForMember(x => x.Id, exp => exp.Ignore())
                 .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                 .ForMember(x => x.CreatedBy, exp => exp.Ignore());

            this.CreateMap<PropertyProfileCreateArgs, PropertyProfileCreateCommand>()
                 .ForMember(x => x.Id, exp => exp.Ignore())
                 .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                 .ForMember(x => x.CreatedBy, exp => exp.Ignore());

            this.CreateMap<PropertyProfileUpdateArgs, PropertyProfileUpdateCommand>()
                 .ForMember(x => x.Id, exp => exp.Ignore())
                 .ForMember(x => x.CreatedOn, exp => exp.Ignore())
                 .ForMember(x => x.CreatedBy, exp => exp.Ignore());
        }
    }
}
