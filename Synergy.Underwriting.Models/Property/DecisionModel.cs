using System;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.Property
{
    public class DecisionModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public FastEntityModel<int> Type { get; set; }

        public string Comment { get; set; }

        public DecisionLevelModel Level { get; set; }

        public DateTime? DecisionDate { get; set; }
    }
}
