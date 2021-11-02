using System;
using Synergy.Underwriting.DAL.Queries.Entities.History;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyValuationAudit : PropertyValuationBase, IHistoryAuditModel<Guid>
    {
        public DateTime InsertedOn { get; set; }

        public Guid InsertedBy { get; set; }

        public Guid OperationId { get; set; }
    }
}