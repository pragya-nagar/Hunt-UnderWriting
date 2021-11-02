using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models
{
    public class DataCutRuleModel : IResultModel
    {
        public Guid EventId { get; set; }

        public Guid Id { get; set; }

        public ResultType ResultType { get; set; }

        public IEnumerable<DataCutItemModel> Items { get; set; }
    }
}