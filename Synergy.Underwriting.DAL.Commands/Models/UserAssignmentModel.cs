using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class UserAssignmentModel
    {
        public Guid UserId { get; set; }

        public int AssignmentsCount { get; set; }
    }
}