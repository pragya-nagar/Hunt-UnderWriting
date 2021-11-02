using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class DecisionLevelProfileModel
    {
        public Guid Id { get; set; }

        public Guid PropertyProfileId { get; set; }

        public Guid EventDecisionLevelId { get; set; }

        public int Order { get; set; }

        public List<DecisionLevelUserAssignmentModel> UsersAssignment { get; set; }
    }
}
