using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class SetEventLockStatusCommand : Command
    {
        public Guid EventId { get; set; }
    }
}
