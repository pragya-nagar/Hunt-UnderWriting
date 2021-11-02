using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.Property
{
    public class PropertyAssignmentModel : PropertyModel
    {
        public Guid PropertyId { get; set; }

        public int? Scoring { get; set; }

        public IEnumerable<int> DisplayStrategy { get; set; }

        public FastEntityModel<int> GeneralLandUseCode { get; set; }

        public FastEntityModel<int> InternalLandUseCode { get; set; }

        public int DelinquencyYear { get; set; }

        public FastEntityModel<int> County { get; set; }

        public EventModel Event { get; set; }

        public string LandStateCode { get; set; }

        public string ImprovementStateCode { get; set; }

        public bool? IsHomestead { get; set; }

        public decimal TaxRatio { get; set; }

        public string TaxId { get; set; }

        public string CadId { get; set; }

        public bool ThirdPartyForeclosure { get; set; }

        public bool Bankruptcy { get; set; }

        public bool Veteran { get; set; }

        public bool PaymentPlan { get; set; }

        public bool Mortgage { get; set; }

        public bool DisabilityExemption { get; set; }

        public bool Over65SurvivingSpouse { get; set; }

        public string LegalDescription { get; set; }

        public float? LandAcres { get; set; }

        public int? BuildingSqFt { get; set; }

        public int? YearBuilt { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public bool IsLatestPropertyData { get; set; }

        public SupplementalModel Supplemental { get; set; }

        public IEnumerable<DecisionModel> Decisions { get; set; }

        public DateTime CreatedOn { get; set; }

        public FastEntityModel<Guid> CreatedBy { get; set; }

        public DateTime ModifiedOn { get; set; }

        public FastEntityModel<Guid> ModifiedBy { get; set; }

        public DateTime? DeletedOn { get; set; }

        public FastEntityModel<int> DataCutDecision { get; set; }
    }
}
