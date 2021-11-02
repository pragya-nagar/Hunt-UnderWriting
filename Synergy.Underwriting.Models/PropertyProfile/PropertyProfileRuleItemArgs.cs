using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileRuleItemArgs
    {
        public int PropertyProfileRuleFieldId { get; set; }

        public int PropertyProfileLogicTypeId { get; set; }

        public IEnumerable<PropertyProfileRuleItemValueModel> PropertyProfileRuleItemValues { get; set; }
    }
}