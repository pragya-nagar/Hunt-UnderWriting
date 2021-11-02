using System;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Enum;

namespace Synergy.Underwriting.DAL.Queries.Original.Models.Assigment
{
    public class DecisionModel : AuditModel
    {
        public Guid Id { get; set; }

        public Guid DelinquencyId { get; set; }

        public Guid UserId { get; set; }

        public DecisionType? Value { get; set; }
    }
}
