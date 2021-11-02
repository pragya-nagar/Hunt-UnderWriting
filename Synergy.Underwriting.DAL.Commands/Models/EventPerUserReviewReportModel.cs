using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class EventPerUserReviewReportModel
    {
        public Guid EventId { get; set; }

        public Guid ReviewerId { get; set; }

        public string ReviewerFirstName { get; set; }

        public string ReviewerLastName { get; set; }

        public string EventNumber { get; set; }

        public string County { get; set; }

        public int Assigned { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Unreviewed { get; set; }

        public int Research { get; set; }

        public int ReviewsCompleted { get; set; }

        public decimal ReviewedPercent { get; set; }

        public decimal UnreviewedPercent { get; set; }

        public Guid LevelId { get; set; }

        public string Level { get; set; }

        public int LevelOrder { get; set; }
    }
}
