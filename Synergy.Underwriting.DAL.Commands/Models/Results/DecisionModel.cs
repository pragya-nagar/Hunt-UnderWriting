using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class DecisionModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int? DecisionTypeId { get; set; }

        public Guid LevelId { get; set; }

        public bool IsFinal { get; set; }

        public int Order { get; set; }
    }
}