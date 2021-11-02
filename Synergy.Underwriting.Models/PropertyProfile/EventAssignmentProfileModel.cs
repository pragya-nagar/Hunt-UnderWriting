using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class EventAssignmentProfileModel : IResultModel
    {
        public int TotalPropertyCount { get; set; }

        public int OtherPropertyCount { get; set; }

        public int AutoProcessedCount { get; set; }

        public List<AssignmentPropertyProfileModel> PropertyProfiles { get; set; }
    }
}
