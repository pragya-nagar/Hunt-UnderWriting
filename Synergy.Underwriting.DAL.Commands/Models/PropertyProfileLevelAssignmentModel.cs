using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class PropertyProfileLevelAssignmentModel
    {
        public Guid? PropertyProfileId { get; set; }

        public int? ProfileOrder { get; set; }

        public List<UserAssignmentModel> UsersAssignment { get; set; }
    }
}