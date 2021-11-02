using System;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models
{
    public class DelinquencyCommentModel : IResultModel
    {
        public Guid Id { get; set; }

        public Guid DelinquencyId { get; set; }

        public FastEntityModel<Guid> Author { get; set; }

        public string Comment { get; set; }

        public DateTime CommentDate { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
