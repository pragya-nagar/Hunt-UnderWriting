using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class AssignmentLevelUpdateCommand : Command
    {
        public Guid EventId { get; set; }

        public IEnumerable<AssigmentUpdateModel> Assigments { get; set; }
    }
}
