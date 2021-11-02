using System;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class LeadModel
    {
        public Guid Id { get; set; }

        public string AccountName { get; set; }

        public bool DoNotContact { get; set; }

        public LeadAddressModel Address { get; set; }
    }
}
