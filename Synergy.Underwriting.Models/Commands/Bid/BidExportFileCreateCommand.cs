using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class BidExportFileCreateCommand : Command
    {
        public Guid EventId { get; set; }

        public string FileName { get; set; }
    }
}
