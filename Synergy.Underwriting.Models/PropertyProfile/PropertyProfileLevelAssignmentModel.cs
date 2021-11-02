using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileLevelAssignmentModel
    {
        public Guid? PropertyProfileId { get; set; }

        public int? ProfileOrder { get; set; }

        public IEnumerable<UserAssignmentsModel> UsersAssignment { get; set; }
    }
}
