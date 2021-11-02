using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class EventStateModel
    {
        public Guid EventId { get; set; }

        public int StateId { get; set; }
    }
}
