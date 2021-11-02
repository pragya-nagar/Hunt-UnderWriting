using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class EventModel
    {
        public Guid EventId { get; set; }

        public int CountyId { get; set; }
    }
}
