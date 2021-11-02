using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Queries.Original.Models.Assigment
{
    public class EventAssignmentModel
    {
        public Guid Id { get; set; }

        public int DelinquencyAmount { get; set; }

        public int AutoProcessedAmount { get; set; }

        public IEnumerable<EventAssignmentLevelModel> Levels { get; set; }
    }
}
