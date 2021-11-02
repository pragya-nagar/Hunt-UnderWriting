using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models.Assigment
{
    public class EventAssignmentLevelModel : IModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }

        public int AvailableRecords { get; set; }

        public IDictionary<Guid, EventLevelUserAssignmentModel> Assignment { get; set; }
    }
}
