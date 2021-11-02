using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Models.PropertyProfile;

namespace Synergy.Underwriting.Models.Commands.EventAssignment
{
    public class EventAssignmentCreateCommand : Command
    {
        public Guid EventId { get; set; }

        public IEnumerable<LevelAssignmentModel> LevelAssignments { get; set; }
    }
}
