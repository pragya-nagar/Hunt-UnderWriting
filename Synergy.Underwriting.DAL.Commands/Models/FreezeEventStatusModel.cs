using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class FreezeEventStatusModel
    {
        public IEnumerable<Guid> EventIds { get; set; }

        public bool NeedToFreeze { get; set; }
    }
}
