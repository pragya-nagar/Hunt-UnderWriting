using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class EventDataCutDecision : IAuditEntity<Guid>
    {
        public Guid EventDataCutStrategyId { get; set; }

        public Guid DelinquencyId { get; set; }

        public EventDataCutStrategy EventDataCutStrategy { get; set; }

        public Delinquency Delinquency { get; set; }

        public int DecisionTypeId { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public Guid Id { get; set; }
    }
}
