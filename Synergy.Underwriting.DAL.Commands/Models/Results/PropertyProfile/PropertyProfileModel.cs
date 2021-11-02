using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class PropertyProfileModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<Guid> RuleIds { get; set; }

        public IEnumerable<int> StateIds { get; set; }
    }
}
