using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.Results.MailMerge
{
    public class MailMergePropertyModel
    {
        public Guid DeliquencyId { get; set; }

        public int DelinquencyYear { get; set; }

        public decimal TaxRatio { get; set; }

        public decimal AmountDue { get; set; }

        public decimal PropertyAmountDue { get; set; }

        public decimal LeadAmountDue { get; set; }

        public string LandUseCode { get; set; }

        public string GeneralLandUseCode { get; set; }

        public string InternalLandUseCode { get; set; }

        public decimal RUAmount { get; set; }

        public decimal RULTV { get; set; }

        public decimal LTV { get; set; }

        public IEnumerable<string> DisplayStrategies { get; set; }

        public string Owner { get; set; }

        public string PropertyAddress { get; set; }

        public string PropertyCity { get; set; }

        public string PropertyState { get; set; }

        public int? PropertyStateId { get; set; }

        public string PropertyZipCode { get; set; }

        public string ParcelId { get; set; }

        public bool? Homestead { get; set; }

        public decimal AppraisedValue { get; set; }

        public decimal LeadAppraisedValue { get; set; }

        public decimal LandValue { get; set; }

        public decimal BuildingValue { get; set; }

        public float LandAcres { get; set; }

        public int BuildingSqFt { get; set; }

        public int? YearBuilt { get; set; }

        public DateTime? LastSaleDate { get; set; }

        public decimal LastSaleAmount { get; set; }

        public int OpenLiens { get; set; }

        public int ClosedLiens { get; set; }

        public string RecentBuyerName { get; set; }

        public decimal RecentBuyerRate { get; set; }

        public string Comment { get; set; }

        public decimal PropertyRating { get; set; }

        public decimal AreaRating { get; set; }

        public bool? Occupied { get; set; }

        public string RoofCondition { get; set; }

        public string LawnMaintained { get; set; }

        public string MailingAddress1 { get; set; }

        public string MailingAddress2 { get; set; }

        public string MailingAddress3 { get; set; }

        public string MailingCity { get; set; }

        public string MailingState { get; set; }

        public string MailingZipCode { get; set; }

        public string AdvertisementBatch { get; set; }

        public string AdvertisementNumber { get; set; }

        public string FinalDecision { get; set; }

        public string FinalReason { get; set; }

        public string FinalReviewer { get; set; }

        public string BidNumber { get; set; }

        public string CertNo { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Overbid { get; set; }

        public decimal Premium { get; set; }

        public decimal RecoverableFees { get; set; }

        public decimal NonRecoverableFees { get; set; }

        public decimal ResultInterestRate { get; set; }

        public decimal PenaltyRate { get; set; }

        public string PurchasingEntity { get; set; }

        public string Portfolio { get; set; }

        public decimal TotalPurchaseAmount { get; set; }

        public ExportMortgageModel Mortgage1 { get; set; }

        public ExportMortgageModel Mortgage2 { get; set; }

        public string LegalDescription { get; internal set; }

        public Guid? EventId { get; set; }

        public bool? DoNotContact { get; set; }

        public MailMergeCampaignModel Campaign { get; set; }
    }
}