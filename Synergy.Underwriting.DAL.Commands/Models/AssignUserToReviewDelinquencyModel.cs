using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class AssignUserToReviewDelinquencyModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid DelinquencyId { get; set; }

        public Guid EventDecisionLevelId { get; set; }
    }
}
