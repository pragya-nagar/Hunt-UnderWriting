using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.Comment
{
    public class DelinquencyCommentCreateCommand : Command
    {
        public Guid DelinquencyId { get; set; }

        public string Comment { get; set; }
    }
}
