using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class AttachFileToPropertyCommand : IAttachFileToPropertyCommand
    {
        private ISynergyContext _context;

        public AttachFileToPropertyCommand(ISynergyContext context)
        {
            this._context = context;
        }

        public void Dispatch(AttachFileToPropertyModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).GetAwaiter().GetResult();
        }

        public async Task<int> DispatchAsync(AttachFileToPropertyModel entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var delinquency = await this._context.Delinquency
                .Select(x => new { x.Id, x.PropertyId })
                .FirstOrDefaultAsync(x => x.Id == entity.DelinquencyId, cancellationToken)
                .ConfigureAwait(false);

            if (delinquency == null)
            {
                throw new ApplicationException("Invalid delinquency id.");
            }

            var data = new PropertyAttachment()
                {
                    Id = entity.Id,
                    ContentType = entity.ContentType,
                    FileCreatedOn = DateTime.UtcNow,
                    Path = entity.Path,
                    FileName = entity.FileName,
                    PropertyAttachmentTypeId = (int)entity.AttachmentType,
                    PropertyId = delinquency.PropertyId,
                }.OnCreateAudit(userId);

            this._context.PropertyAttachment.Add(data);

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
