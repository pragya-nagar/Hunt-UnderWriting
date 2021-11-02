using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileRuleItemModel
    {
        public FastEntityModel<int> PropertyProfileRuleField { get; set; }

        public FastEntityModel<int> PropertyProfileLogicType { get; set; }

        public IEnumerable<FastEntityModel<Guid>> PropertyProfileRuleItemValues { get; set; }
    }
}