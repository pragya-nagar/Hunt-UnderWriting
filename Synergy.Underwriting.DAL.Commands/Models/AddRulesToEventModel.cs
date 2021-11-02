using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class AddRulesToEventModel
    {
        public Guid EventId { get; set; }

        public IEnumerable<Guid> DataCutRuleIds { get; set; }
    }
}
