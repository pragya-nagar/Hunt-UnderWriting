using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.Event
{
    public class ReviewDumpCreateCommand : Command
    {
        public string FileName { get; set; }

        public int StateId { get; set; }

        public DateTime? SaleDateFrom { get; set; }

        public DateTime? SaleDateTo { get; set; }

        public bool IsPerUserReport { get; set; }

        public bool IsEventLocked { get; set; }
    }
}
