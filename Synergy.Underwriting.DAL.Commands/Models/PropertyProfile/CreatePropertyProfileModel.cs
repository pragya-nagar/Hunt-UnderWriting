using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreatePropertyProfileModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<Guid> PropertyProfileRuleIds { get; set; }

        public IEnumerable<int> StateIds { get; set; }
    }
}
