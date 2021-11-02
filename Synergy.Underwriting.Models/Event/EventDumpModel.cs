using System;
using System.Collections.Generic;
using System.Linq;

namespace Synergy.Underwriting.Models
{
    public class EventDumpModel
    {
        public EventDumpModel(int mortgageCount, int levelCount)
        {
            this.Mortgage = Enumerable.Repeat(new MortgageDumpModel(), mortgageCount);
            this.Level = Enumerable.Repeat(new LevelDumpModel(), levelCount);
        }

        public EventDumpModel()
        {
        }

        public string InternalDelinquencyId { get; set; }

        public string LegalDescription { get; set; }

        public string EventNumber { get; set; }

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

        public string TreasurerName { get; set; }

        public string TreasurerEmail { get; set; }

        public decimal InterestRate { get; set; }

        public string AuctionAddress { get; set; }

        public DateTime? AuctionStartTime { get; set; }

        public decimal EstimatedDepositAmount { get; set; }

        public decimal EstimatedPurchaseAmount { get; set; }

        public decimal RefundAmount { get; set; }

        public int DelinquencyYear { get; set; }

        public decimal TaxRatio { get; set; }

        public decimal AmountDue { get; set; }

        public string LandUseCode { get; set; }

        public string GeneralLandUseCode { get; set; }

        public string InternalLandUseCode { get; set; }

        public decimal RUAmount { get; set; }

        public decimal LTV { get; set; }

        public string DisplayStrategies { get; set; }

        public string Owner { get; set; }

        public string PropertyAddress { get; set; }

        public string PropertyCity { get; set; }

        public string PropertyState { get; set; }

        public string PropertyZipCode { get; set; }

        public string ParcelId { get; set; }

        public bool? Homestead { get; set; }

        public decimal AppraisedValue { get; set; }

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

        public string CurrentDecisionLevel { get; set; }

        public string CurrentDecision { get; set; }

        public string CurrentDecisionReason { get; set; }

        public string CurrentDecisionReviewer { get; set; }

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

        public int? Scoring { get; set; }

        public IEnumerable<MortgageDumpModel> Mortgage { get; set; }

        public IEnumerable<LevelDumpModel> Level { get; set; }
    }
}





//public async Task<List<LoanApplicantsExcelModel>> GetDBViewExcelLoanList()
//{
//    var listresult = await AllLoanData();
//    var loanList = listresult.GroupBy(x => new { x.LoanNumber }).Select(y => y.FirstOrDefault()).ToList();

//    var propList = from p in listresult
//                   select new ProcessingProperty
//                   {
//                       PropertyAddress = p.Address,
//                       PropertyId = p.PropertyId,
//                       LoanNumber = p.LoanNumber,
//                       StateId = p.StateId,
//                       StateName = p.StateName,
//                       City = p.City,
//                       ZipCode = p.ZipCode,
//                       CountyId = p.CountyId,
//                       FullAddress = p.FullAddress,
//                       CountyName = p.CountyName
//                   };

//    propList = propList.GroupBy(x => new { x.PropertyId, x.LoanNumber }).Select(y => y.FirstOrDefault()).ToList();

//    var list = from l in loanList
//               join p in propList on new { l.PropertyId, l.LoanNumber } equals new { p.PropertyId, p.LoanNumber }
//               select new LoanApplicantsExcelModel
//               {
//                   LoanNumber = l.LoanNumber,
//                   BorrowerName = l.BorrowerName,
//                   SubjectPropertyAddress = p.FullAddress,
//                   PropertyType = l.PropertyType,
//                   NewMoneyDollar = l.NewMoney.FormatCurrencyNullable(),
//                   LoanAmountDollar = l.LoanAmount.FormatCurrencyNullable(),
//                   LoanAmount = l.LoanAmount.Value,
//                   NewMoney = l.NewMoney.Value,
//                   UnderwritingConditions = l.UnderwritingConditions,
//                   LoanOfficer = l.LoanOfficer,
//                   LeadSource = l.LeadSource,
//                   FileStartDate = l.FileStartDate.FormatDate_MMddyyyy(),
//                   ProcessingActionItems = l.ProcessingActionItems,
//                   TaxRequestDate = l.TaxRequestDate.FormatDate_MMddyyyy(),
//                   FileClosingDate = l.FileClosingDate.FormatDate_MMddyyyy(),
//                   LienHolder = l.LienHolder,
//                   NumberOfLienHolders = l.NumberOfLienHolders,
//                   PhoneNumber = l.PhoneNumber,
//                   Email = l.Email,
//                   Transferee = l.Transferee,
//                   InterestRatePercent = l.InterestRate.FormatPercentage(),
//                   InterestRate = l.InterestRate,
//                   CurrentMileStone = l.MilestoneDesc,
//                   FolderId = l.FolderId

//               };
//    return list.GroupBy(x => x.LoanNumber).Select(y => y.FirstOrDefault()).ToList();
//}