using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class Event : IAuditEntity<Guid>
    {
        public string EventNumber { get; set; }

        public int CountyId { get; set; }

        public County County { get; set; }

        public int StateId { get; set; }

        public State State { get; set; }

        public int AuctionTypeId { get; set; }

        public AuctionType AuctionType { get; set; }

        public int EventTypeId { get; set; }

        public EventType EventType { get; set; }

        public bool IsLocked { get; set; }

        public DateTime SaleDate { get; set; }

        public DateTime? FundingDate { get; set; }

        public bool IsRejectReasonRequired { get; set; }

        public DateTime DueDate { get; set; }

        public IEnumerable<Bid> Bids { get; set; }

        public IEnumerable<Delinquency> Delinquencies { get; set; }

        public IEnumerable<EventDecisionLevel> DecisionLevels { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public bool IsFreezed { get; set; }
    }
}
