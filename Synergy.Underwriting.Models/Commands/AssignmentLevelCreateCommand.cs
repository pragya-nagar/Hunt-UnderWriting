using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class AssignmentLevelCreateCommand : Command
    {
        public Guid EventId { get; set; }

        public IEnumerable<AssigmentCreateModel> Assigments { get; set; }
    }
}
