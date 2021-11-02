using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class LevelModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public int OtherPropertyCount { get; set; }

        public IEnumerable<LevelPropertyProfileAssignmensModel> Assignments { get; set; }
    }
}
