using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class RuleUpdateCommand : Command
    {
        public string Name { get; set; }

        public int CountyId { get; set; }

        public int DataCutResultTypeId { get; set; }

        public IEnumerable<RuleItemModel> RuleItems { get; set; }
    }
}
