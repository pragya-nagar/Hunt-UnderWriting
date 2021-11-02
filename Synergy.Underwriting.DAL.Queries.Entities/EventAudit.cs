using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class EventAudit : IAuditEntity<Guid>
    {
        public DateTime InsertedOn { get; set; }

        public Guid InsertedBy { get; set; }

        public Guid OperationId { get; set; }

        public string EventNumber { get; set; }

        public int CountyId { get; set; }

        public int StateId { get; set; }

        public int AuctionTypeId { get; set; }

        public int EventTypeId { get; set; }

        public bool IsLocked { get; set; }

        public DateTime SaleDate { get; set; }

        public DateTime? FundingDate { get; set; }

        public bool IsRejectReasonRequired { get; set; }

        public DateTime DueDate { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public bool IsFreezed { get; set; }
    }
}
