using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class EventDecisionLevelUser : IAuditEntity<Guid>
    {
        public Guid Id { get; set; }

        public int AssigmentCount { get; set; }

        public Guid EventDecisionLevelId { get; set; }

        public Guid? EventDecisionLevelPropertyProfileId { get; set; }

        public Guid UserId { get; set; }

        public EventDecisionLevel EventDecisionLevel { get; set; }

        public EventDecisionLevelPropertyProfile EventDecisionLevelPropertyProfile { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
