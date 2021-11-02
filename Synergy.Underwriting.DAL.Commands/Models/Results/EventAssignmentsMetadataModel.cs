using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class EventAssignmentsMetadataModel
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public DateTime SaleDate { get; set; }

        public DateTime? FundingDate { get; set; }

        public int StateId { get; set; }

        public int TypeId { get; set; }

        public bool IsLocked { get; set; }

        public int ManualDelinquencyCount { get; set; }

        public IEnumerable<(int Index, int Level, IEnumerable<(Guid UserId, DateTime? DecisionDate)> Users)> NLevelUsers { get; set; }

        public IEnumerable<(Guid UserId, DateTime? DecisionDate)> FinalLevelUsers { get; set; }

        public IDictionary<int, IEnumerable<Guid>> DepartmentUserIds { get; set; }
    }
}
