using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class BidUpdateCommand : Command
    {
        public string Number { get; set; }

        public string Entity { get; set; }

        public string Portfolio { get; set; }

        public Guid EventId { get; set; }
    }
}