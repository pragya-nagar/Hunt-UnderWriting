using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.Common.Exceptions;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class UpdateDelinquencyCommentCommand : IUpdateDelinquencyCommentCommand
    {
        private readonly ISynergyContext _context;

        public UpdateDelinquencyCommentCommand(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispatch(UpdateDelinquencyCommentModel comment, Guid userId)
        {
            this.DispatchAsync(comment, userId).GetAwaiter().GetResult();
        }

        public async Task<int> DispatchAsync(UpdateDelinquencyCommentModel comment, Guid userId, CancellationToken cancellationToken = default)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }

            var entity = await this._context.DelinquencyComment
                .FirstOrDefaultAsync(x => x.Id == comment.Id, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                throw new NotFoundException();
            }

            entity.Comment = comment.Comment;
            entity.OnModifyAudit(userId);

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}