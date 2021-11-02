using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class Bid : IAuditEntity<Guid>
    {
        public string Number { get; set; }

        public string Entity { get; set; }

        public string Portfolio { get; set; }

        public Guid EventId { get; set; }

        public Event Event { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
