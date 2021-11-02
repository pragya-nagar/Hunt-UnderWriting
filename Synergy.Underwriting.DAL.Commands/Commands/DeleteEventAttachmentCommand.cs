using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class DeleteEventAttachmentCommand : IDeleteEventAttachmentCommand
    {
        private readonly ISynergyContext _context;

        public DeleteEventAttachmentCommand(ISynergyContext context)
        {
            this._context = context;
        }

        public void Dispatch(DeleteAttachmentModel model, Guid userId)
        {
            var entity = this._context.EventAttachment.Single(x => x.Id == model.Id).OnDeleteAudit(userId);

            this._context.EventAttachment.Update(entity);
            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(DeleteAttachmentModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = await _context.EventAttachment.SingleAsync(x => x.Id == model.Id, cancellationToken).ConfigureAwait(false);
            entity.OnDeleteAudit(userId);

            this._context.EventAttachment.Update(entity);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
