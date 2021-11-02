using System.Collections.Generic;

namespace Synergy.Underwriting.Models.Rule
{
    public class CreateRuleArgs
    {
        public string RuleName { get; set; }

        public int CountyId { get; set; }

        public int DataCutResultTypeId { get; set; }

        public IEnumerable<CreateRuleItemArgs> RuleItems { get; set; }
    }
}
