using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class OrderedProfileArgs
    {
        public Guid EventId { get; set; }

        public List<PropertyProfileOrderModel> ProfileOrders { get; set; }
    }
}
