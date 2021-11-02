using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.Comment
{
    public class DelinquencyCommentUpdateCommand : Command
    {
        public string Comment { get; set; }
    }
}