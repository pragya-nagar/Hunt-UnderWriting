using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class DelinquencyPropertyDisplayStrategyBase : IAuditEntity<Guid>
    {
        public Guid DelinquencyId { get; set; }

        public int PropertyDisplayStrategyId { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }

    public class DelinquencyPropertyDisplayStrategy : DelinquencyPropertyDisplayStrategyBase
    {
        public PropertyDisplayStrategy PropertyDisplayStrategy { get; set; }

        public Delinquency Delinquency { get; set; }
    }
}
