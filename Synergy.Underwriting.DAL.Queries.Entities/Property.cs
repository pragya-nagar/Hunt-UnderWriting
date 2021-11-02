using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyBase : IAuditEntity<Guid>
    {
        public string ParcelId { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public int StateId { get; set; }

        public Guid LeadId { get; set; }

        public Lead Lead { get; set; }

        public string LeadName => this.Lead.AccountName;

        public int? GeneralLandUseCodeId { get; set; }

        public int? InternalLandUseCodeId { get; set; }

        public string LandUseCode { get; set; }

        public bool? Homestead { get; set; }

        public float? LandAcres { get; set; }

        public int? BuildingSqFt { get; set; }

        public int? YearBuilt { get; set; }

        public decimal TotalAmountDue { get; set; }

        public State State { get; set; }

        public string StateName => this.State?.Name;

        public int CountyId { get; set; }

        public County County { get; set; }

        public string CountyName
        {
            get
            {
                return County?.Name;
            }
        }

        public GeneralLandUseCode GeneralLandUseCode { get; set; }

        public string GeneralLandUseCodeName
        {
            get
            {
                return GeneralLandUseCode?.Name;
            }
        }

        public InternalLandUseCode InternalLandUseCode { get; set; }

        public string InternalLandUseCodeName
        {
            get
            {
                return InternalLandUseCode?.Description;
            }
        }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public string PropertyBK { get; set; }

        public string CADId { get; set; }

        public string TAXId { get; set; }

        public string FolioId { get; set; }

        public string LegalDescription { get; set; }

        public string LandStateCode { get; set; }

        public string ImprovementStateCode { get; set; }

        public bool Over65SurvivingSpouse { get; set; }

        public bool DisabilityExemption { get; set; }

        public bool Mortgage { get; set; }

        public bool PaymentPlan { get; set; }

        public bool Veteran { get; set; }

        public bool Bankruptcy { get; set; }

        public bool ThirdPartyForeclosure { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public decimal LastYearDue { get; set; }
    }

    public class Property : PropertyBase
    {
        public IEnumerable<PropertyAttachment> Attachments { get; set; }

        public IEnumerable<Delinquency> Delinquencies { get; set; }
    }
}
