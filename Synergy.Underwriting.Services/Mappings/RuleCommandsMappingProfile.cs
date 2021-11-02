using System;
using AutoMapper;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.PropertyProfile;

namespace Synergy.Underwriting.Services.Mappings
{
    public class RuleCommandsMappingProfile : Profile
    {
        public RuleCommandsMappingProfile()
        {
            this.CreateMap<RulesToEventUpdateCommand, DAL.Commands.Models.AddRulesToEventModel>()
                .ForMember(x => x.DataCutRuleIds, exp => exp.MapFrom(src => src.RuleIds));

            this.CreateMap<RulesToEventAttachCommand, DAL.Commands.Models.AttachRulesToEventModel>()
                .ForMember(x => x.DataCutRuleIds, exp => exp.MapFrom(src => src.RuleIds));

            this.CreateMap<PropertyProfileRuleCreateCommand, DAL.Commands.Models.CreatePropertyProfileRuleModel>();
            this.CreateMap<PropertyProfileRuleItemModel, DAL.Commands.Models.PropertyProfile.PropertyProfileRuleItemModel>();
            this.CreateMap<PropertyProfileRuleItemValueModel, DAL.Commands.Models.PropertyProfile.PropertyProfileRuleItemValueModel>();

            this.CreateMap<RuleCreateCommand, Underwriting.DAL.Commands.Models.CreateDataCutRuleModel>(MemberList.Destination)
                .ForMember(x => x.Id, exp => exp.MapFrom(src => src.Id))
                .ForMember(x => x.DataCutRuleItems, exp => exp.MapFrom(src => src.RuleItems))
                ;

            this.CreateMap<RuleItemModel, Underwriting.DAL.Commands.Models.DataCutRuleItemModel>()
                .ForMember(x => x.Id, exp => exp.MapFrom(src => Guid.NewGuid()))
                ;

            this.CreateMap<RuleItem, Underwriting.DAL.Commands.Models.DataCutRuleItemModel>()
                .ForMember(x => x.Id, exp => exp.MapFrom(src => Guid.NewGuid()))
                .ForMember(x => x.DataCutLogicTypeId, exp => exp.MapFrom(src => src.DataCutLogicTypeId))
                .ForMember(x => x.DataCutRuleFieldId, exp => exp.MapFrom(src => src.DataCutRuleFieldId))
                .ForMember(x => x.Value, exp => exp.MapFrom(src => src.Value))
                ;
        }
    }
}
