using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.PropertyProfile
{
    public class PropertyProfileCreateCalculatingCommand : Command
    {
        public Guid ProfileId { get; set; }

        public IEnumerable<int> StateIds { get; set; }
    }
}
