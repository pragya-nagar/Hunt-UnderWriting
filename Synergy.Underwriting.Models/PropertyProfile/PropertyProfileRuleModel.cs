using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileRuleModel : IResultModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<PropertyProfileRuleItemModel> PropertyProfileRuleItems { get; set; }
    }
}
