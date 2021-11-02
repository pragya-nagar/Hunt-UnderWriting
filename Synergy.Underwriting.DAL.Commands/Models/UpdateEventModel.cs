using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class UpdateEventModel
    {
        public Guid Id { get; set; }

        public string EventNumber { get; set; }

        public int? CountyId { get; set; }

        public string CountyName { get; set; }

        public DateTime SaleDate { get; set; }

        public DateTime DueDate { get; set; }

        public string CurrentTask { get; set; }

        public string Progress { get; set; }

        public DateTime? FundingDate { get; set; }

        public DateTime? RegistrationDeadline { get; set; }

        public DateTime? DepositDeadline { get; set; }

        public decimal? DepositAmount { get; set; }

        public decimal? TreasurerFee { get; set; }

        public decimal? InterestRate { get; set; }

        public string AuctionAddress { get; set; }

        public DateTime? AuctionStartTime { get; set; }

        public string TreasurerName { get; set; }

        public string TreasurerEmail { get; set; }

        public decimal? EstimatedPurchaseAmount { get; set; }

        public decimal? EstimatedDepositAmount { get; set; }

        public decimal? RefundAmount { get; set; }

        public bool IsLocked { get; set; }

        public IEnumerable<UserDepartmentsModel> UserDepartments { get; set; }

        public int StateId { get; set; }

        public int EventTypeId { get; set; }

        public int AuctionTypeId { get; set; }

        public int SaleDateStatusId { get; set; }

        public int? EventEntityId { get; set; }

        public int? FinalPaymentTypeId { get; set; }

        public Guid UserId { get; set; }

        public bool IsRejectReasonRequired { get; set; }
    }
}
