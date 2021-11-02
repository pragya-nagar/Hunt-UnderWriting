using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.Comment
{
    public class CommentFileProcessCommand : Command
    {
        public Guid EventId { get; set; }

        public string FileName { get; set; }
    }
}