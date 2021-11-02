using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.Abstracts;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class BulkCreateCommentCommand : IBulkCreateCommentCommand
    {
        private readonly ISynergyContext _context;
        private readonly IClockService _clockService;

        public BulkCreateCommentCommand(ISynergyContext context, IClockService clockService)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._clockService = clockService ?? throw new ArgumentNullException(nameof(clockService));
        }

        public void Dispatch(IEnumerable<CreateCommentModel> model, Guid userId)
        {
            this.DispatchAsync(model, userId).GetAwaiter().GetResult();
        }

        public async Task<int> DispatchAsync(IEnumerable<CreateCommentModel> model, Guid userId, CancellationToken cancellationToken = default)
        {
            if (model?.Any() != true)
            {
                return 0;
            }

            var commentDate = this._clockService.UtcNow;

            var entityList = model.Select(x => new DelinquencyComment
            {
                Id = x.Id,
                DelinquencyId = x.DelinquencyId,
                AuthorId = userId,
                Comment = x.Comment,
                CommentDate = commentDate,
            }.OnCreateAudit(userId)).ToList();

            await this._context.DelinquencyComment.AddRangeAsync(entityList, cancellationToken).ConfigureAwait(false);

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}