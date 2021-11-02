using System;
using Synergy.DataAccess.Enum;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class SetUserDecisionModel
    {
        public string Comment { get; set; }

        public Guid DecisionId { get; set; }

        public DecisionType Decision { get; set; }

        public DateTime? DecisionDate { get; set; }
    }
}
