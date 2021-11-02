using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class SupplementalDataBase : IAuditEntity<Guid>
    {
        public string AdvertisementNumber { get; set; }

        public string AdvertisementBatch { get; set; }

        public string AssessorURL { get; set; }

        public string TreasurerURL { get; set; }

        public string GisURL { get; set; }

        public Guid DelinquencyId { get; set; }

        public DateTime? LastSaleDate { get; set; }

        public float? LastSaleAmount { get; set; }

        public float? MortgageLoanAmount1 { get; set; }

        public float? MortgageLoanAmount2 { get; set; }

        public string MortgageLender1 { get; set; }

        public string MortgageLender2 { get; set; }

        public DateTime? MortgageOriginationDate1 { get; set; }

        public DateTime? MortgageMaturityDate1 { get; set; }

        public DateTime? MortgageOriginationDate2 { get; set; }

        public DateTime? MortgageMaturityDate2 { get; set; }

        public int? OpenLiens { get; set; }

        public int? ClosedLiens { get; set; }

        public string RecentBuyerName { get; set; }

        public decimal? RecentBuyerRate { get; set; }

        public string InspectorComment { get; set; }

        public decimal? InspectorPropertyRating { get; set; }

        public decimal? InspectorAreaRating { get; set; }

        public bool? InspectorOccupied { get; set; }

        public string InspectorRoofCondition { get; set; }

        public string InspectorLawnMaintained { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }

    public class SupplementalData : SupplementalDataBase
    {
        public Delinquency Delinquency { get; set; }
    }
}
