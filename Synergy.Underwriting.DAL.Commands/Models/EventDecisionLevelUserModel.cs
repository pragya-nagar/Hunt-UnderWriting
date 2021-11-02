using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class EventDecisionLevelUserModel
    {
        public Guid UserId { get; set; }

        public int AssigmentCount { get; set; }
    }
}
