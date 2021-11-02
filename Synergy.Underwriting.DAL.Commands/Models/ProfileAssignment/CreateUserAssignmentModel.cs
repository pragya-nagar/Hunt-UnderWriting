using System;

namespace Synergy.Underwriting.DAL.Commands.Models.ProfileAssignment
{
    public class CreateUserAssignmentModel
    {
        public Guid UserId { get; set; }

        public int AssignmentsCount { get; set; }
    }
}
