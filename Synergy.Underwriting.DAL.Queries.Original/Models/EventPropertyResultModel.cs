namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventPropertyResultModel
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

        public EventPropertyBidModel Bid { get; set; }
    }
}
