using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreatePropertyProfileDelinquencyModel
    {
        public Guid DelinquencyId { get; set; }

        public Guid PropertyProfileId { get; set; }
    }
}
