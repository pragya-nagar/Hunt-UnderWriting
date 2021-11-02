using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class ResultBulkUpdateCommand : Command
    {
        public Guid EventId { get; set; }

        public IEnumerable<Result> List { get; set; }
    }
}