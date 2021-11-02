using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class ApplyRulesCommand : Command
    {
        public Guid EventId { get; set; }

        public IEnumerable<Guid> RuleIds { get; set; }
    }
}