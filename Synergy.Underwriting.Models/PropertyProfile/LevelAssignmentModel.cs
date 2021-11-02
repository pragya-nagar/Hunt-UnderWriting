using System;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class LevelAssignmentModel : EventAssignmentCreateArgs
    {
        public Guid LevelId { get; set; }
    }
}
