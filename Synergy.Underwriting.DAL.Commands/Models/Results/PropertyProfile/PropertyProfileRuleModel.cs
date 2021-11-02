using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class PropertyProfileRuleModel
    {
        public Guid Id { get; set; }

        public IEnumerable<PropertyProfileRuleItem> Items { get; set; }
    }
}
