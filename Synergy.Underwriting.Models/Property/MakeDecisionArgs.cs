using System;

namespace Synergy.Underwriting.Models.Property
{
    public class MakeDecisionArgs
    {
        public Guid LevelId { get; set; }

        public DataAccess.Enum.DecisionType Decision { get; set; }

        public string Comment { get; set; }
    }
}
