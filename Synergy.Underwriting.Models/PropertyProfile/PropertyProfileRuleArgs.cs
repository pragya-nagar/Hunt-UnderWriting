using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileRuleArgs
    {
        public string Name { get; set; }

        public IEnumerable<PropertyProfileRuleItemArgs> PropertyProfileRuleItems { get; set; }
    }
}