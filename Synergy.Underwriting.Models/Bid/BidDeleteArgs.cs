using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.Bid
{
    public class BidDeleteArgs
    {
        public IEnumerable<Guid> BidIds { get; set; }
    }
}
