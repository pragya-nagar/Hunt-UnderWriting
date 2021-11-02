using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class EventDumpFileCreateCommand : Command
    {
        public Guid EventId { get; set; }

        public IEnumerable<EventDumpField> Fields { get; set; }

        public string FileName { get; set; }
    }
}