using System;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class LeadAudit : LeadBase
    {
        public DateTime InsertedOn { get; set; }

        public Guid InsertedBy { get; set; }

        public Guid OperationId { get; set; }

        public State State { get; set; }
    }
}
