using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class EventAssignmentCreateArgs
    {
        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }

        public IEnumerable<PropertyProfileLevelAssignmentModel> Assignments { get; set; }
    }
}
