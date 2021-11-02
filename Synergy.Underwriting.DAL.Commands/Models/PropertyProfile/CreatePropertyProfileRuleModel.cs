using System;
using System.Collections.Generic;
using Synergy.Underwriting.DAL.Commands.Models.PropertyProfile;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreatePropertyProfileRuleModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<PropertyProfileRuleItemModel> PropertyProfileRuleItems { get; set; }
    }
}
