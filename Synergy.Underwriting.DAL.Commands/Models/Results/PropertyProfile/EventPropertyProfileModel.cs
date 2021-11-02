using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class EventPropertyProfileModel
    {
        public Guid EventId { get; set; }

        public List<Guid> PropertyProfileIds { get; set; }
    }
}
