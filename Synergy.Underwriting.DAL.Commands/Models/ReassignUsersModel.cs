using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class ReassignUsersModel
    {
        public Guid LevelId { get; set; }

        public List<(Guid userId, Guid decisionId)> Assignments { get; set; }
    }
}
