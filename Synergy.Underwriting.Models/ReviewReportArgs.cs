using System;

namespace Synergy.Underwriting.Models
{
    public class ReviewReportArgs
    {
        public int StateId { get; set; }

        public DateTime? SaleDateFrom { get; set; }

        public DateTime? SaleDateTo { get; set; }

        public bool IsPerUserReport { get; set; }

        public bool IsEventLocked { get; set; }
    }
}
