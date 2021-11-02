using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands.Comment
{
    public class DelinquencyCommentDeleteCommand : Command
    {
        public string Comment { get; set; }
    }
}