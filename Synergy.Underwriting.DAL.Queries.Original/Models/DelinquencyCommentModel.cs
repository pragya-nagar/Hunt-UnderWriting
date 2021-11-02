using System;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class DelinquencyCommentModel : IModel
    {
        public Guid Id { get; set; }

        public Guid DelinquencyId { get; set; }

        public FastEntityModel<Guid> Author { get; set; }

        public string Comment { get; set; }

        public DateTime CommentDate { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
