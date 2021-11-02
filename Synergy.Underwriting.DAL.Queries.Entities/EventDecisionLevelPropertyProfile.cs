using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class EventDecisionLevelPropertyProfile : IAuditEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid PropertyProfileId { get; set; }

        public Guid EventDecisionLevelId { get; set; }

        public int Order { get; set; }

        public EventDecisionLevel EventDecisionLevel { get; set; }

        public PropertyProfile PropertyProfile { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
