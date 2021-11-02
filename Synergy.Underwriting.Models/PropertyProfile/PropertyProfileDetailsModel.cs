using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileDetailsModel : PropertyProfileModel
    {
        public IEnumerable<PropertyProfileRuleModel> PropertyProfileRules { get; set; }
    }
}