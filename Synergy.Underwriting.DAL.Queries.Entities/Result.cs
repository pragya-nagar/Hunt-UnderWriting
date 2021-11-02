using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class Result : IAuditEntity<Guid>
    {
        public string BidNumber { get; set; }

        public string CertNo { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal? Overbid { get; set; }

        public decimal? Premium { get; set; }

        public decimal InterestRate { get; set; }

        public decimal? PenaltyRate { get; set; }

        public decimal? RecoverableFees { get; set; }

        public decimal? NonRecoverableFees { get; set; }

        public Guid DelinquencyId { get; set; }

        public Delinquency Delinquency { get; set; }

        public Guid? BidId { get; set; }

        public Bid Bid { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
