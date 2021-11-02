using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.Property
{
    public class DecisionHistoryModel : IResultModel
    {
        public decimal AssessedValue { get; set; }

        public decimal RUAmount { get; set; }

        public decimal RULTV { get; set; }

        public FastEntityModel<Guid> DecisionUser { get; set; }

        public string DecisionComment { get; set; }

        public string DecisionLevel { get; set; }

        public int DecisionLevelOrder { get; set; }

        public DateTime? DecisionDate { get; set; }

        public FastEntityModel<int> Decision { get; set; }
    }
}
