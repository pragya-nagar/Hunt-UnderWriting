using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetCommentAuthorIdQuery : SingleQuery<Guid, Guid?>
    {
        private readonly ISynergyContext _context;

        public GetCommentAuthorIdQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<Guid?> ExecuteAsync(Guid args, CancellationToken cancellationToken = default)
        {
            var comment = await this._context.DelinquencyComment
                .Select(x => new { x.Id, x.AuthorId })
                .FirstOrDefaultAsync(x => x.Id == args, cancellationToken)
                .ConfigureAwait(false);

            return comment?.AuthorId;
        }
    }
}