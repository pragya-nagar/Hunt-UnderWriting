using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class BidDeleteCommand : Command
    {
        public IEnumerable<Guid> BidIds { get; set; }
    }
}