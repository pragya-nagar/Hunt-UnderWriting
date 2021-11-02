using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class EventReviewReportModel
    {
        public Guid EventId { get; set; }

        public string EventNumber { get; set; }

        public string County { get; set; }

        public int TotalCount { get; set; }

        public int BulkRejected { get; set; }

        public int BulkApproved { get; set; }

        public int ReviewAvailable { get; set; }

        public int Assigned { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Unreviewed { get; set; }

        public int Research { get; set; }

        public int ReviewsCompleted { get; set; }

        public decimal ReviewedPercent { get; set; }

        public decimal UnreviewedPercent { get; set; }

        public decimal AutoDecisionPercent { get; set; }

        public decimal AssignedPercent { get; set; }

        public decimal UnassignedPercent { get; set; }
    }
}
