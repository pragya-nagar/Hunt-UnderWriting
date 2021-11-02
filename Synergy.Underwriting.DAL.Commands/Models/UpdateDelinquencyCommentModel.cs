using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class UpdateDelinquencyCommentModel
    {
        public Guid Id { get; set; }

        public string Comment { get; set; }
    }
}