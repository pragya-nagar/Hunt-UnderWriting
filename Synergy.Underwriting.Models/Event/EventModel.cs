using System;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models
{
    public class EventModel : IResultModel
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public FastEntityModel<int> County { get; set; }

        public FastEntityModel<int> State { get; set; }

        public FastEntityModel<int> Type { get; set; }

        public FastEntityModel<int> AuctionType { get; set; }

        public DateTime? FundingDate { get; set; }

        public DateTime SaleDate { get; set; }

        public string CurrentTask { get; set; }

        public DateTime? DueDate { get; set; }

        public string Progress { get; set; }

        public bool IsLocked { get; set; }

        public bool IsAssigned { get; set; }

        public bool IsRejectReasonRequired { get; set; }

        public FastEntityModel<Guid> AssignedTo { get; set; }
    }
}