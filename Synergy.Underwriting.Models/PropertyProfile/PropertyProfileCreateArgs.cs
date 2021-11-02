using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileCreateArgs
    {
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<Guid> PropertyProfileRuleIds { get; set; }

        public IEnumerable<int> StateIds { get; set; }
    }
}