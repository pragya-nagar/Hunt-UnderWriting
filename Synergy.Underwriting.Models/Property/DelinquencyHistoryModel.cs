using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models.Property
{
    public class DelinquencyHistoryModel : IResultModel
    {
        public Guid Id { get; set; }

        public int Year { get; set; }

        public string EventNumber { get; set; }

        public DateTime EventSaleDate { get; set; }

        public IList<DecisionHistoryModel> Decisions { get; set; }
    }
}
