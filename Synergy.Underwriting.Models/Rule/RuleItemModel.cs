using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.Rule
{
    public class RuleItemModel
    {
        public string Value { get; set; }

        public FastEntityModel<int> DataCutLogicType { get; set; }

        public FastEntityModel<int> DataCutRuleField { get; set; }
    }
}