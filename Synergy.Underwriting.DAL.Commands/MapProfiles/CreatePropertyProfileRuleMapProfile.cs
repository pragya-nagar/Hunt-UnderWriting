using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Models.PropertyProfile;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreatePropertyProfileRuleMapProfile : Profile
    {
        public CreatePropertyProfileRuleMapProfile()
        {
            CreateMap<CreatePropertyProfileRuleModel, PropertyProfileRule>()
                .IgnoreAuditMembers()
                .ForMember(x => x.PropertyProfileRulePropertyProfiles, t => t.Ignore());

            CreateMap<PropertyProfileRuleItemModel, PropertyProfileRuleItem>()
                .IgnoreAuditMembers()
                .ForMember(x => x.Id, t => t.Ignore())
                .ForMember(x => x.PropertyProfileRule, t => t.Ignore())
                .ForMember(x => x.PropertyProfileLogicType, t => t.Ignore())
                .ForMember(x => x.PropertyProfileRuleField, t => t.Ignore())
                .ForMember(x => x.PropertyProfileRuleId, t => t.Ignore());

            CreateMap<PropertyProfileRuleItemValueModel, PropertyProfileRuleItemValue>()
                .IgnoreAuditMembers()
                .ForMember(x => x.Id, t => t.Ignore())
                .ForMember(x => x.PropertyProfileRuleItem, t => t.Ignore())
                .ForMember(x => x.PropertyProfileRuleItemId, t => t.Ignore());
        }
    }
}
