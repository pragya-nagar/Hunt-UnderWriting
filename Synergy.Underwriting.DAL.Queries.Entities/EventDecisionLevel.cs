using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class EventDecisionLevel : IAuditEntity<Guid>
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public bool IsFinal { get; set; }

        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public List<EventDecisionLevelUser> EventDecisionLevelUser { get; set; }

        public List<Decision> Decisions { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
