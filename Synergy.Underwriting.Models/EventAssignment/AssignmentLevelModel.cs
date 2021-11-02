using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models.EventAssignment
{
    public class AssignmentLevelModel : IResultModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }

        public int AvailableRecords { get; set; }

        public IDictionary<Guid, UserAssignmentModel> Assignment { get; set; }
    }
}
