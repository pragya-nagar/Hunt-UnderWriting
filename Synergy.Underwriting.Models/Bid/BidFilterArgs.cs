using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.Bid
{
    public class BidFilterArgs
    {
        public IEnumerable<Guid> EventIds { get; set; }
    }
}
