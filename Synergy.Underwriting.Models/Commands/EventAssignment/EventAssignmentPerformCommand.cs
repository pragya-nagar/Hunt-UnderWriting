using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.Event
{
    public class EventAssignmentPerformCommand : Command
    {
        public Guid EventId { get; set; }
    }
}