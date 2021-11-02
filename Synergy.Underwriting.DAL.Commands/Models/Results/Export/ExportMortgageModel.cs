using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class ExportMortgageModel
    {
        public decimal Loan { get; set; }

        public DateTime? MaturityDate { get; set; }
    }
}