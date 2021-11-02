using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class LevelAssignmentModel
    {
        public Guid LevelId { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }

        public List<PropertyProfileLevelAssignmentModel> Assignments { get; set; }
    }
}