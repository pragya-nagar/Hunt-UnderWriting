using System;
using Synergy.DataAccess.Enum;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class LevelDecisionModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid DelinquencyId { get; set; }

        public Guid LevelId { get; set; }

        public DecisionType? DecisionType { get; set; }
    }
}
