using AutoMapper;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Domain.Mappings
{
    public class RuleMappingProfile : Profile
    {
        public RuleMappingProfile()
        {
            this.CreateMap<DAL.Queries.Original.Models.DataCutLogicTypeModel, DataCutLogicTypeModel>();

            this.CreateMap<DAL.Queries.Original.Models.DataCutRuleFieldModel, DataCutRuleFieldModel>();
        }
    }
}
