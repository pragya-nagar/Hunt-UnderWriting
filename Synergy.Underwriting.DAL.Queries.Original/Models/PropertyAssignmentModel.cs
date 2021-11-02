using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Enum;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class PropertyAssignmentModel : AuditModel, IModel
    {
        public Guid Id { get; set; }

        public Guid PropertyId { get; set; }

        public FastEntityModel<int> County { get; set; }

        public GeneralLandUseCode GeneralLandUseCode { get; set; }

        public FastEntityModel<int> InternalLandUseCode { get; set; }

        public string CADId { get; set; }

        public string TAXId { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public FastEntityModel<int> State { get; set; }

        public string ZipCode { get; set; }

        public string LegalDescription { get; set; }

        public string LandStateCode { get; set; }

        public string ImprovementStateCode { get; set; }

        public string LandUseCode { get; set; }

        public bool Over65SurvivingSpouse { get; set; }

        public bool DisabilityExemption { get; set; }

        public bool Mortgage { get; set; }

        public bool PaymentPlan { get; set; }

        public bool Veteran { get; set; }

        public bool Bankruptcy { get; set; }

        public bool ThirdPartyForeclosure { get; set; }

        public LeadModel Lead { get; set; }

        public string ParcelId { get; set; }

        public bool? Homestead { get; set; }

        public int? PropertyScoring { get; set; }

        public float? LandAcres { get; set; }

        public int? BuildingSqFt { get; set; }

        public int? YearBuilt { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public int DelinquencyYear { get; set; }

        public bool IsLatestPropertyData { get; set; }

        // TODO: ETL values
        public decimal? LTV { get; set; }

        public decimal? RULTV { get; set; }

        public decimal? RUAmount { get; set; }

        public decimal TotalAmountDue { get; set; }

        public decimal LastYearDue { get; set; }

        public EventModel Event { get; set; }

        public List<PropertyDecisionModel> Decisions { get; set; }

        public List<PropertyValuationModel> PropertyValuations { get; set; }

        public PropertySupplementalEventDataModel PropertySupplementalEventData { get; set; }

        public List<int> PropertyDisplayStrategiesIds { get; set; }

        public List<PropertyAttachmentModel> PropertyAttachments { get; set; }

        public FastEntityModel<int> DataCutDecision { get; set; }
    }
}
