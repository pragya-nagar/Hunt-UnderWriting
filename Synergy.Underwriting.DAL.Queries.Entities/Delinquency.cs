using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class DelinquencyBase : IAuditEntity<Guid>
    {
        public int Year { get; set; }

        public decimal Amount { get; set; }

        public decimal? RUAmount { get; set; }

        public decimal? RULTVPercent { get; set; }

        public decimal? LTVPercent { get; set; }

        public Guid PropertyId { get; set; }

        public Guid EventId { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }

    public class Delinquency : DelinquencyBase
    {
        public Property Property { get; set; }

        public Event Event { get; set; }

        public Result Result { get; set; }

        public SupplementalData SupplementalData { get; set; }

        public IEnumerable<Decision> Decisions { get; set; }

        public IEnumerable<EventDataCutDecision> EventDataCutDecisions { get; set; }

        public IEnumerable<PropertyProfileDelinquency> PropertyProfileDelinquencies { get; set; }

        public IEnumerable<DelinquencyPropertyDisplayStrategy> DelinquencyPropertyDisplayStrategy { get; set; }
    }
}
