using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class DecisionLevelModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public bool IsFinal { get; set; }
    }
}
