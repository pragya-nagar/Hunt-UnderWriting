using AutoMapper;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Rule;

namespace Synergy.Underwriting.API.Mappings
{
    public class RuleMappingProfile : Profile
    {
        public RuleMappingProfile()
        {
            this.CreateMap<CreateRuleArgs, RuleCreateCommand>(MemberList.Source)
                .ForMember(x => x.Name, exp => exp.MapFrom(src => src.RuleName))
                ;

            this.CreateMap<CreateRuleItemArgs, Models.Commands.RuleItemModel>(MemberList.Source)
                ;

            this.CreateMap<RuleItemArgs, Models.Commands.RuleItem>()
                .ForMember(x => x.DataCutLogicTypeId, exp => exp.MapFrom(src => src.DataCutLogicTypeId))
                .ForMember(x => x.DataCutRuleFieldId, exp => exp.MapFrom(src => src.DataCutRuleFieldId))
                .ForMember(x => x.Value, exp => exp.MapFrom(src => src.Value));
        }
    }
}
