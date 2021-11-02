using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class PropertyProfileRuleItem
    {
        public PropertyProfileRuleField Field { get; set; }

        public PropertyProfileLogicType Logic { get; set; }

        public IEnumerable<string> Values { get; set; }
    }
}