using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.PropertyProfile
{
    public class PropertyProfileCreateCommand : Command
    {
        public bool IsActive { get; set; }

        public string Name { get; set; }

        public IEnumerable<Guid> PropertyProfileRuleIds { get; set; }

        public IEnumerable<int> StateIds { get; set; }
    }
}
