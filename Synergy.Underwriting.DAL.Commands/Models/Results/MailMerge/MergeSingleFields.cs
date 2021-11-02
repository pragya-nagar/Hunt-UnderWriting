using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results.MailMerge
{
    public class MergeSingleFields
    {
        public Guid InternalDelinquencyId { get; set; }

        public string Event { get; set; }

        public string State { get; set; }

        public string County { get; set; }

        public string EventType { get; set; }

        public string AuctionType { get; set; }

        public DateTime? FundingDate { get; set; }

        public DateTime? SaleDate { get; set; }

        public string SaleDateStatus { get; set; }

        public DateTime? RegistrationDeadline { get; set; }

        public DateTime? DepositDeadline { get; set; }

        public decimal DepositAmount { get; set; }

        public string FinalPaymentType { get; set; }

        public decimal TreasurerFee { get; set; }

        public decimal InterestRate { get; set; }

        public string AuctionAddress { get; set; }

        public DateTime? AuctionStartTime { get; set; }

        public string TreasurerName { get; set; }

        public string TreasurerEmail { get; set; }

        public decimal EstimatedPurchaseAmount { get; set; }

        public decimal EstimatedDepositAmount { get; set; }

        public decimal RefundAmount { get; set; }

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

        public string DisplayStrategies { get; set; }

        public string Owner { get; set; }

        public string PropertyAddress { get; set; }

        public string PropertyCity { get; set; }

        public string PropertyState { get; set; }

        public string PropertyZipCode { get; set; }

        public string ParcelID { get; set; }

        public string Homestead { get; set; }

        public decimal LandValue { get; set; }

        public decimal AppraisedValue { get; set; }

        public decimal LeadAppraisedValue { get; set; }

        public decimal BuildingValue { get; set; }

        public float LandAcres { get; set; }

        public int BuildingSqFt { get; set; }

        public int? YearBuilt { get; set; }

        public DateTime? LastSaleDate { get; set; }

        public decimal LastSaleAmount { get; set; }

        public decimal Mortgage1Loan { get; set; }

        public DateTime? Mortgage1Date { get; set; }

        public decimal Mortgage2Loan { get; set; }

        public DateTime? Mortgage2Date { get; set; }

        public int OpenLiens { get; set; }

        public int ClosedLiens { get; set; }

        public string RecentBuyerName { get; set; }

        public decimal RecentBuyerRate { get; set; }

        public string LegalDescription { get; set; }

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

        public string BidNumber { get; set; }

        public string CertNo { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Overbid { get; set; }

        public decimal Premium { get; set; }

        public decimal RecoverableFees { get; set; }

        public decimal NonRecoverableFees { get; set; }

        public decimal PenaltyRate { get; set; }

        public string PurchasingEntity { get; set; }

        public string Portfolio { get; set; }

        public decimal TotalPurchaseAmount { get; set; }

        public string AdvertisementBatch { get; set; }

        public string AdvertisementNumber { get; set; }

        public string CampaignName { get; set; }

        public string CampaignType { get; set; }

        public string CampaignSubType { get; set; }

        public string CreatedDate { get; set; }

        public string Description { get; set; }

        public string TargetDate { get; set; }

        public string Note { get; set; }

        public string AssignedUser { get; set; }

        public string DoNotContact { get; set; }
    }
}
