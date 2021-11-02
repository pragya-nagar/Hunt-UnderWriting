using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.PropertyProfile
{
    public class PropertyProfileRuleCreateCommand : Command
    {
        public string Name { get; set; }

        public IEnumerable<PropertyProfileRuleItemModel> PropertyProfileRuleItems { get; set; }
    }
}
