using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreateDelinquencyCommentModel
    {
        public Guid Id { get; set; }

        public Guid DelinquencyId { get; set; }

        public string Comment { get; set; }
    }
}
