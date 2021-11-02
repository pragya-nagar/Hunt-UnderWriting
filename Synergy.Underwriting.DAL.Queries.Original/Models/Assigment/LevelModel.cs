using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models.Assigment
{
    public class LevelModel : AuditModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }

        public List<DecisionModel> Decisions { get; set; }
    }
}
