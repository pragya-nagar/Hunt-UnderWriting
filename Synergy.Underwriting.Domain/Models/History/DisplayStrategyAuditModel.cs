using System;
using Synergy.Underwriting.DAL.Queries.Entities;
using Synergy.Underwriting.DAL.Queries.Entities.History;

namespace Synergy.Underwriting.Domain.Models.History
{
    public class DisplayStrategyAuditModel : DelinquencyPropertyDisplayStrategyAudit, IHistoryAuditModel<Guid>
    {
        public string DispositionStrategy { get; set; }
    }
}
