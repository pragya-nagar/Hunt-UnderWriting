using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.Property
{
    public class SupplementalModel
    {
        public string AdvertisementBatch { get; set; }

        public decimal? RecentBuyerRate { get; set; }

        public string RecentBuyerName { get; set; }

        public int? ClosedLiens { get; set; }

        public int? OpenLiens { get; set; }

        public string InspectorLawnMaintained { get; set; }

        public string InspectorRoofCondition { get; set; }

        public bool? InspectorOccupied { get; set; }

        public decimal? InspectorAreaRating { get; set; }

        public decimal? InspectorPropertyRating { get; set; }

        public string InspectorComment { get; set; }

        public decimal? LastSaleAmount { get; set; }

        public DateTime? LastSaleDate { get; set; }

        public string AdvertisementNumber { get; set; }

        public string AssessorURL { get; set; }

        public string TreasurerURL { get; set; }

        public string GisURL { get; set; }

        public IEnumerable<MortgageModel> MortgageList { get; set; }
    }
}
