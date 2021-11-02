using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;
using AuctionType = Synergy.DataAccess.Enum.AuctionType;
using EventEntity = Synergy.DataAccess.Enum.EventEntity;
using EventType = Synergy.DataAccess.Enum.EventType;
using FinalPaymentType = Synergy.DataAccess.Enum.FinalPaymentType;
using SaleDateStatus = Synergy.DataAccess.Enum.SaleDateStatus;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventModel : AuditModel, IModel
    {
        public Guid Id { get; set; }

        public string EventNumber { get; set; }

        public FastEntityModel<int> County { get; set; }

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

        public bool IsAssigned { get; set; }

        public List<UserDepartmentModel> UserDepartments { get; set; }

        public FastEntityModel<int> State { get; set; }

        public IEnumerable<FastEntityModel<Guid>> Attachments { get; set; }

        public IEnumerable<EventRulesModel> EventDataCutRules { get; set; }

        public EventType EventType { get; set; }

        public AuctionType AuctionType { get; set; }

        public SaleDateStatus SaleDateStatus { get; set; }

        public EventEntity? EventEntity { get; set; }

        public FinalPaymentType? FinalPaymentType { get; set; }

        public int? OriginalListCount { get; set; }

        public decimal? OriginalListAmount { get; set; }

        public bool IsRejectReasonRequired { get; set; }
    }
}
