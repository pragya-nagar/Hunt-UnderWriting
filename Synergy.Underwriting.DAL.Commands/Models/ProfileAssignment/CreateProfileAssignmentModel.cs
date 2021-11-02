using System;
using System.Collections.Generic;
using Synergy.Underwriting.DAL.Commands.Models.ProfileAssignment;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreateProfileAssignmentModel
    {
        public Guid EventId { get; set; }

        public Guid ProfileId { get; set; }

        public List<Guid> PreviousProfiles { get; set; }

        public Guid EventDecisionLevelId { get; set; }

        public int Order { get; set; }

        public List<CreateUserAssignmentModel> UserAssignment { get; set; }
    }
}
