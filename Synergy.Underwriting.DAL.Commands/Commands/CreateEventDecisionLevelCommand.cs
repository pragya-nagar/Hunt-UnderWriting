using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class CreateEventDecisionLevelCommand : ICreateEventDecisionLevelCommand
    {
        private readonly ISynergyContext _context;

        public CreateEventDecisionLevelCommand(ISynergyContext context)
        {
            this._context = context;
        }

        public void Dispatch(CreateEventDecisionLevelModel entity, Guid userId)
        {
            var level = new EventDecisionLevel
            {
                Id = entity.Id,
                EventId = entity.EventId,
                Name = entity.Name,
                Order = entity.Order,
                IsFinal = entity.IsFinal,
            }.OnCreateAudit(userId);

            this._context.EventDecisionLevel.Add(level);
            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(CreateEventDecisionLevelModel entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var level = new EventDecisionLevel
            {
                Id = entity.Id,
                EventId = entity.EventId,
                Name = entity.Name,
                Order = entity.Order,
                IsFinal = entity.IsFinal,
            }.OnCreateAudit(userId);

            await _context.EventDecisionLevel.AddAsync(level, cancellationToken).ConfigureAwait(false);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
