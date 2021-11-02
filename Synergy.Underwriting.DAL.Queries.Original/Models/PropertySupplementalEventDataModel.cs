using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class PropertySupplementalEventDataModel
    {
        public DateTime? LastSaleDate { get; set; }

        public decimal? LastSaleAmount { get; set; }

        public string InspectorComment { get; set; }

        public decimal? InspectorPropertyRating { get; set; }

        public decimal? InspectorAreaRating { get; set; }

        public bool? InspectorOccupied { get; set; }

        public string InspectorRoofCondition { get; set; }

        public string InspectorLawnMaintained { get; set; }

        public int? OpenLiens { get; set; }

        public int? ClosedLiens { get; set; }

        public string RecentBuyerName { get; set; }

        public decimal? RecentBuyerRate { get; set; }

        public string AdvertisementBatch { get; set; }

        public string AdvertisementNumber { get; set; }

        public string AssessorURL { get; set; }

        public string TreasurerURL { get; set; }

        public string GisURL { get; set; }

        public List<MortgageDataModel> MortgageData { get; set; }
    }
}
