using System;
using System.Collections.Generic;
using System.Text;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities.History
{
    public interface IHistoryAuditModel<T> : IAuditEntity<T>
    {
        DateTime InsertedOn { get; set; }

        Guid InsertedBy { get; set; }

        Guid OperationId { get; set; }
    }
}
