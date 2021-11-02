using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models.Assigment
{
    public class AssigmentModel : IModel
    {
        public Guid EventId { get; set; }

        public int DelinquencyAmount { get; set; }

        public IEnumerable<EventDataCutDecisionModel> DataCutDecisions { get; set; }

        public IEnumerable<LevelModel> Levels { get; set; }
    }
}
