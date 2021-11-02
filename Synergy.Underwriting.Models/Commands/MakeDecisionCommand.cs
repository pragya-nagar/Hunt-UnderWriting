using System;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class MakeDecisionCommand : Command
    {
        public DecisionType Decision { get; set; }

        public string Comment { get; set; }

        public Guid LevelId { get; set; }
    }
}