using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class EventDataCutStrategy : IAuditEntity<Guid>
    {
        public Guid EventId { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<EventDataCutRule> EventDataCutRuleLinks { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
