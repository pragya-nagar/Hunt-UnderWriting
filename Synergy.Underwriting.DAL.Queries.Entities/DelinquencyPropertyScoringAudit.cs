using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class DelinquencyPropertyScoringAudit : IAuditEntity<Guid>
    {
        public DateTime InsertedOn { get; set; }

        public Guid InsertedBy { get; set; }

        public Guid OperationId { get; set; }

        public Guid DelinquencyId { get; set; }

        public int? PropertyScoring { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
