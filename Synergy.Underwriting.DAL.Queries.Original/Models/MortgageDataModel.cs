using System;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class MortgageDataModel
    {
        public int MortgageDataNumber { get; set; }

        public string MortgageLender { get; set; }

        public decimal? MortgageLoanAmount { get; set; }

        public DateTime? MortgageOriginationDate { get; set; }

        public DateTime? MortgageMaturityDate { get; set; }
    }
}