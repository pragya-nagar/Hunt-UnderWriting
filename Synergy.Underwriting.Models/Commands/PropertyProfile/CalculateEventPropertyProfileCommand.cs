using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.PropertyProfile
{
    public class CalculateEventPropertyProfileCommand : Command
    {
        public Guid EventId { get; set; }

        public List<Guid> PropertyProfileIds { get; set; }

        public PropertyProfileCalculationTriggerType Type { get; set; }
    }
}
