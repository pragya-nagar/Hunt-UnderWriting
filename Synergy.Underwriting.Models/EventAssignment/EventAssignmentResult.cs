using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models.EventAssignment
{
    public class EventAssignmentResult : IResultModel
    {
        public int DelinquencyAmount { get; set; }

        public int AutoProcessedAmount { get; set; }

        public IEnumerable<AssignmentLevelModel> Levels { get; set; }
    }
}
