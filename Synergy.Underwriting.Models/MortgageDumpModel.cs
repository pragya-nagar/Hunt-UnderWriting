using System;

namespace Synergy.Underwriting.Models
{
    public class MortgageDumpModel
    {
        public decimal Loan { get; set; }

        public DateTime? MaturityDate { get; set; }
    }
}
