using System;

namespace Synergy.Underwriting.Models.Commands
{
    public class Result
    {
        public Guid Id { get; set; }

        public Guid DelinquencyId { get; set; }

        public string BidNumber { get; set; }

        public string CertNo { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal? Overbid { get; set; }

        public decimal? Premium { get; set; }

        public decimal InterestRate { get; set; }

        public decimal? PenaltyRate { get; set; }

        public decimal? RecoverableFees { get; set; }

        public decimal? NonRecoverableFees { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }
    }
}