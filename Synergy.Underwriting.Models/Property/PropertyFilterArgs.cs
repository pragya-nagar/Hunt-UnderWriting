using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.Property
{
    public class PropertyFilterArgs
    {
        public Guid? EventId { get; set; }

        public Guid? CurrentDelenquencyId { get; set; }

        public bool MoveForward { get; set; }

        public bool? AssignmentByUser { get; set; }

        public Guid? LevelId { get; set; }

        public DataAccess.Enum.ReviewStatusFilters? ReviewStatus { get; set; }

        public DataAccess.Enum.ReviewDecisionSearchField? ReviewDecision { get; set; }

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

        public List<int> InternalLandUseCodes { get; set; }

        public List<int> GeneralLandUseCodes { get; set; }
    }
}
