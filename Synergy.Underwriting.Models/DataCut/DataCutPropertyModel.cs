using System;

namespace Synergy.Underwriting.Models
{
    public class DataCutPropertyModel
    {
        public Guid Id { get; set; }

        public string AccountName { get; set; }

        public string PropertyAddress { get; set; }

        public string PropertyZipCode { get; set; }

        public string LandUseCode { get; set; }

        public string GeneralLandUseCode { get; set; }

        public decimal LandValue { get; set; }

        public string ImprovementValue { get; set; }

        public decimal AppraisedValue { get; set; }

        public decimal AmountDue { get; set; }

        public decimal RUAmountDue { get; set; }

        public int OpenLiens { get; set; }

        public int ClosedLiens { get; set; }

        public decimal LTVPercent { get; set; }

        public decimal HorizonLTVPercent { get; set; }

        public decimal RULTVPercent { get; set; }

        public decimal TaxRatioPercent { get; set; }

        public int StateId { get; set; }
    }
}