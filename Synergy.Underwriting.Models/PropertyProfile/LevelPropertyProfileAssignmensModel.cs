using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class LevelPropertyProfileAssignmensModel
    {
        public Guid? PropertyProfileId { get; set; }

        public int? ProfileOrder { get; set; }

        public int PropertyCount { get; set; }

        public IEnumerable<PropertyProfileUserAssignmentModel> UsersAssignment { get; set; }
    }
}
