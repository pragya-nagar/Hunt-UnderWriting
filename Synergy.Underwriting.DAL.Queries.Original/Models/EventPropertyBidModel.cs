using System;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventPropertyBidModel
    {
        public Guid Id { get; set; }

        public string Portfolio { get; set; }

        public string PurchasingEntity { get; set; }
    }
}
