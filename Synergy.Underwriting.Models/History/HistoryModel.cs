using System;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models
{
    public class HistoryModel : IResultModel
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public DateTime DateTime { get; set; }

        public string Field { get; set; }

        public string PreviousValue { get; set; }

        public string NewValue { get; set; }
    }
}