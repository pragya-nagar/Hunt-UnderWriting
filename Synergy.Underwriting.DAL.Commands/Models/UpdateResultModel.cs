using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class UpdateResultModel : CreateResultModel
    {
        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }
    }
}
