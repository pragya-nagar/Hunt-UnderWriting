using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.EventAssignment
{
    public class AssignmentLevelCreateArgs
    {
        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }

        public IDictionary<Guid, int> Assignments { get; set; }
    }
}
