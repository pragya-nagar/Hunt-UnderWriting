using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreateOtherAssignmentModel
    {
        public Guid EventId { get; set; }

        public Guid EventDecisionLevelId { get; set; }

        public List<Guid> LevelProfileIds { get; set; }
    }
}
