using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.Rule
{
    public class RuleModel : IResultModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsAttached { get; set; }

        public FastEntityModel<int> DataCutResultType { get; set; }

        public IEnumerable<RuleItemModel> DataCutRuleItems { get; set; }
    }
}
