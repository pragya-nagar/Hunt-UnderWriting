using System;
using Synergy.Underwriting.DAL.Queries.Entities;
using Synergy.Underwriting.DAL.Queries.Entities.History;

namespace Synergy.Underwriting.Domain.Models.History
{
    public class LeadAuditModel : LeadAudit, IHistoryAuditModel<Guid>
    {
        public string MailingState { get; set; }
    }
}
