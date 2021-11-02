using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class EventModel
    {
        public Guid Id { get; set; }

        public int StateId { get; set; }

        public int CountyId { get; set; }

        public int EventTypeId { get; set; }

        public string EventNumber { get; set; }

        public DateTime SaleDate { get; set; }
    }
}
