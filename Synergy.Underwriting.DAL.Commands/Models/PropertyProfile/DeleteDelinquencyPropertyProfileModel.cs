using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class DeleteDelinquencyPropertyProfileModel
    {
        public Guid EventId { get; set; }

        public Guid ProfileId { get; set; }

        public IEnumerable<Guid> DelinquencyIds { get; set; }
    }
}
