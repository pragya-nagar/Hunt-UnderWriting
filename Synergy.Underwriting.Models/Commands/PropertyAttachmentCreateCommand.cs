using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class PropertyAttachmentCreateCommand : Command
    {
        public Guid DelinquencyId { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }
    }
}
