using System;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class EventAssignmentUpdateArgs : EventAssignmentCreateArgs
    {
        public Guid? LevelId { get; set; }
    }
}
