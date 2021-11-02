using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class DelinquencyCalculationModel
    {
        public Guid Id { get; set; }

        public int? GeneralLandUseCodeId { get; set; }

        public int? InternalLandUseCodeId { get; set; }

        public string LandUseCode { get; set; }

        public decimal? TotalAmountDue { get; set; }

        public decimal? AssessedValue { get; set; }

        public decimal? RULTVPercent { get; set; }

        public decimal? LTVPercent { get; set; }
    }
}
