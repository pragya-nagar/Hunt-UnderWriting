using System;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models.Property
{
    public class DecisionLevelModel : IResultModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public bool IsFinal { get; set; }
    }
}
