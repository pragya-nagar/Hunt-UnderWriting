using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class DeleteBidModel
    {
        public IEnumerable<Guid> BidIds { get; set; }
    }
}
