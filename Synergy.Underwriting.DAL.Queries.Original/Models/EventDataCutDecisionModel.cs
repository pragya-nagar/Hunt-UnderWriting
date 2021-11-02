using System;
using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventDataCutDecisionModel : IModel
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public int DecisionTypeId { get; set; }

        public Guid DelinquencyId { get; set; }
    }
}
