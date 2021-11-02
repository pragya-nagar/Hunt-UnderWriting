using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class PropertyFieldsFilterModel
    {
        public decimal? MinAssessedValue { get; set; }

        public decimal? MaxAssessedValue { get; set; }

        public decimal? MinAmountDue { get; set; }

        public decimal? MaxAmountDue { get; set; }

        public string ParcelID { get; set; }

        public string Owner { get; set; }

        public string PropertyAddress { get; set; }

        public string PropertyCity { get; set; }

        public string PropertyZipCode { get; set; }

        public string LandUseCode { get; set; }

        public List<int> GeneralLandUseCodes { get; set; }

        public List<int> InternalLandUseCodes { get; set; }
    }
}