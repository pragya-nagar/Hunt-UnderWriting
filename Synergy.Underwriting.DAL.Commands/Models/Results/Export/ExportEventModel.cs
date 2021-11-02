using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class ExportEventModel
    {
        public Guid Id { get; set; }

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
    }
}