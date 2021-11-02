using System;

namespace Synergy.Underwriting.Models
{
    public class EventFilterArgs
    {
        public int? StateId { get; set; }

        public DataAccess.Enum.EventType? Type { get; set; }

        public DataAccess.Enum.TaskType? TaskType { get; set; }

        public Guid? AssignedTo { get; set; }

        public bool? IsLockedStatus { get; set; }
    }
}