using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.EventAssignment
{
    public class AssignmentLevelUpdateArgs
    {
        public Guid LevelId { get; set; }

        public IDictionary<Guid, int> Assignments { get; set; }
    }
}
