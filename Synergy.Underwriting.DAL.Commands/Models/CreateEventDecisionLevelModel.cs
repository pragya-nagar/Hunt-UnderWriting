using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreateEventDecisionLevelModel
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }
    }
}
