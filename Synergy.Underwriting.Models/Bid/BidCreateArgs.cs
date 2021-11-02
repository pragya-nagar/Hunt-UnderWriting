using System;

namespace Synergy.Underwriting.Models.Bid
{
    public class BidCreateArgs
    {
        public Guid EventId { get; set; }

        public string Number { get; set; }

        public string Entity { get; set; }

        public string Portfolio { get; set; }
    }
}
