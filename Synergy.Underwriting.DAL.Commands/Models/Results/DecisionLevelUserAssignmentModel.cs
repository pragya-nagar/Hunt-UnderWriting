using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class DecisionLevelUserAssignmentModel
    {
        public Guid UserId { get; set; }

        public int AssignmentsCount { get; set; }
    }
}