using System;
using AutoMapper;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands.PropertyProfile;
using Synergy.Underwriting.Models.PropertyProfile;
using PropertyProfileRuleItemModel = Synergy.Underwriting.Models.PropertyProfile.PropertyProfileRuleItemModel;
using PropertyProfileRuleItemValueModel = Synergy.Underwriting.Models.Commands.PropertyProfile.PropertyProfileRuleItemValueModel;

namespace Synergy.Underwriting.Domain.Mappings
{
    public class PropertyProfileMappingProfile : Profile
    {
        public PropertyProfileMappingProfile()
        {
            this.CreateMap<DAL.Queries.Entities.PropertyProfileRule, PropertyProfileRuleModel>()
                ;

            this.CreateMap<DAL.Queries.Entities.PropertyProfileRuleItem, PropertyProfileRuleItemModel>()
                .ForMember(x => x.PropertyProfileRuleItemValues, exp => exp.MapFrom(x => x.PropertyProfileRuleItemValues))
                .ForMember(x => x.PropertyProfileLogicType, exp => exp.MapFrom(x => new FastEntityModel<int>()
                {
                    Id = x.PropertyProfileLogicType.Id,
                    Name = x.PropertyProfileLogicType.Description,
                }))
                .ForMember(x => x.PropertyProfileRuleField, exp => exp.MapFrom(x => new FastEntityModel<int>()
                {
                    Id = x.PropertyProfileRuleField.Id,
                    Name = x.PropertyProfileRuleField.Description,
                }))
            ;

            this.CreateMap<DAL.Queries.Entities.PropertyProfileRuleItemValue, FastEntityModel<Guid>>()
                 .ForMember(x => x.Name, exp => exp.MapFrom(x => x.Value))
                ;

            this.CreateMap<DAL.Queries.Entities.PropertyProfile, PropertyProfileModel>()
                .ForMember(x => x.Id, exp => exp.MapFrom(x => x.Id))
                .ForMember(x => x.IsActive, exp => exp.MapFrom(x => x.IsActive))
                .ForMember(x => x.Name, exp => exp.MapFrom(x => x.Name))
                .ForMember(x => x.States, exp => exp.MapFrom(x => x.PropertyProfileStates))
                ;

            this.CreateMap<DAL.Queries.Entities.PropertyProfileState, FastEntityModel<int>>()
                .ForMember(x => x.Id, exp => exp.MapFrom(x => x.StateId))
                .ForMember(x => x.Name, exp => exp.MapFrom(x => x.State.Abbreviation))
                ;

            this.CreateMap<Underwriting.Models.PropertyProfile.PropertyProfileRuleItemValueModel, PropertyProfileRuleItemValueModel>().ReverseMap();

            this.CreateMap<DAL.Queries.Entities.PropertyProfile, PropertyProfileDetailsModel>()
                .ForMember(x => x.States, exp => exp.MapFrom(x => x.PropertyProfileStates))
                .ForMember(x => x.PropertyProfileRules, exp => exp.MapFrom(x => x.PropertyProfileRulePropertyProfiles))
               ;

            this.CreateMap<DAL.Queries.Entities.PropertyProfileRulePropertyProfile, PropertyProfileRuleModel>()
               .ForMember(x => x.Id, exp => exp.MapFrom(x => x.PropertyProfileRule.Id))
               .ForMember(x => x.Name, exp => exp.MapFrom(x => x.PropertyProfileRule.Name))
               .ForMember(x => x.PropertyProfileRuleItems, exp => exp.MapFrom(x => x.PropertyProfileRule.PropertyProfileRuleItems))
              ;
        }
    }
}
