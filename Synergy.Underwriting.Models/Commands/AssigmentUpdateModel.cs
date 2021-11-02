using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.Commands
{
    public class AssigmentUpdateModel
    {
        public Guid LevelId { get; set; }

        public IDictionary<Guid, int> Assignments { get; set; }
    }
}