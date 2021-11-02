using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models
{
    public class EventDetailsModel : EventModel
    {
        public FastEntityModel<int> SaleDateStatus { get; set; }

        public DateTime? RegistrationDeadline { get; set; }

        public DateTime? DepositDeadline { get; set; }

        public decimal? DepositAmount { get; set; }

        public FastEntityModel<int> FinalPaymentType { get; set; }

        public decimal? TreasurerFee { get; set; }

        public decimal? InterestRate { get; set; }

        public string AuctionAddress { get; set; }

        public DateTime? AuctionStartTime { get; set; }

        public string TreasurerName { get; set; }

        public string TreasurerEmail { get; set; }

        public decimal EstimatedPurchaseAmount { get; set; }

        public decimal EstimatedDepositAmount { get; set; }

        public decimal RefundAmount { get; set; }

        public IEnumerable<FastEntityModel<Guid>> Attachments { get; set; }

        public int? OriginalListCount { get; set; }

        public decimal? OriginalListAmount { get; set; }

        public IEnumerable<UserDepartmentModel> UserDepartments { get; set; }
    }
}