using System;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Enum;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class PropertyDecisionModel : AuditModel, IModel
    {
        public Guid Id { get; set; }

        public string Comment { get; set; }

        public Guid UserId { get; set; }

        public Guid DelinquencyId { get; set; }

        public EventDecisionLevelModel EventDecisionLevel { get; set; }

        public DecisionType? DecisionType { get; set; }

        public DateTime? DecisionDate { get; set; }
    }
}