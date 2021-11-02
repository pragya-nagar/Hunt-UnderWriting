using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class EventAssignmentModel
    {
        public Guid EventId { get; set; }

        public List<LevelAssignmentModel> LevelAssignments { get; set; }
    }
}
