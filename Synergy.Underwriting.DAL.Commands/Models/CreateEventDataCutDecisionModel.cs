using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreateEventDataCutDecisionModel
    {
        public Guid Id { get; set; }

        public int DecisionTypeId { get; set; }

        public Guid DelinquencyId { get; set; }

        public Guid EventDataCutStrategyId { get; set; }
    }
}
