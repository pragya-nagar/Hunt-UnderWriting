using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.PropertyProfile
{
    public class PropertyProfileRuleItemModel
    {
        public int PropertyProfileLogicTypeId { get; set; }

        public int PropertyProfileRuleFieldId { get; set; }

        public IEnumerable<PropertyProfileRuleItemValueModel> PropertyProfileRuleItemValues { get; set; }
    }
}
