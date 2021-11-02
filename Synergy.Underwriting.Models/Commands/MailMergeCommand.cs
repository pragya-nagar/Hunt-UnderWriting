using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class MailMergeCommand : Command
    {
        public string DeliquencyPath { get; set; }

        public Guid TemplateId { get; set; }

        public Guid EventId { get; set; }

        public string ResultPath { get; set; }
    }
}
