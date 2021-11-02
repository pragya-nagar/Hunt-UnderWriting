namespace Synergy.Underwriting.Models
{
    public class ResultCreateArgs
    {
        public string BidNumber { get; set; }

        public string ParcelId { get; set; }

        public string AdvertisementNumber { get; set; }

        public string CertNo { get; set; }

        public int TaxYear { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal? Overbid { get; set; }

        public decimal? Premium { get; set; }

        public decimal InterestRate { get; set; }

        public decimal? PenaltyRate { get; set; }

        public decimal? RecoverableFees { get; set; }

        public decimal? NonRecoverableFees { get; set; }
    }
}
