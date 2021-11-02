using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class Decision : IAuditEntity<Guid>
    {
        public Guid UserId { get; set; }

        public Guid DelinquencyId { get; set; }

        public Delinquency Delinquency { get; set; }

        public Guid? PropertyProfileId { get; set; }

        public int? DecisionTypeId { get; set; }

        public string Comment { get; set; }

        public DateTime? DecisionDate { get; set; }

        public Guid EventDecisionLevelId { get; set; }

        public EventDecisionLevel EventDecisionLevel { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
