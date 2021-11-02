using System;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class DelinquencyPropertyDisplayStrategyAudit : DelinquencyPropertyDisplayStrategyBase
    {
        public DateTime InsertedOn { get; set; }

        public Guid InsertedBy { get; set; }

        public Guid OperationId { get; set; }

        public PropertyDisplayStrategy PropertyDisplayStrategy { get; set; }

        public Delinquency Delinquency { get; set; }
    }
}
