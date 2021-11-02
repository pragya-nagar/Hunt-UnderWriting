using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class EventAssignmentModel
    {
        public int TotalPropertyCount { get; set; }

        public int AutoProcessedCount { get; set; }

        public List<LevelModel> Levels { get; set; }
    }
}
