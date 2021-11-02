using System;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Underwriting.Models.Address;

namespace Synergy.Underwriting.Models
{
    public class LeadModel : IResultModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public AddressModel Address { get; set; }
    }
}
