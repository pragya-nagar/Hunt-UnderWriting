using System;
using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventDecisionLevelModel : IModel
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsFinal { get; set; }
    }
}
