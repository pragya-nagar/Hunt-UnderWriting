using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class SetEventLockStatusCommand : ISetEventLockStatusCommand
    {
        private ISynergyContext _context;

        public SetEventLockStatusCommand(ISynergyContext context)
        {
            _context = context;
        }

        public void Dispatch(Guid entity, Guid userId)
        {
            var eventData = this._context.Event.Single(x => x.Id == entity).OnModifyAudit(userId);
            eventData.IsLocked = !eventData.IsLocked;
            this._context.Event.Update(eventData);
            this._context.SaveChanges();
        }

        public Task<int> DispatchAsync(Guid entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var eventData = this._context.Event.Single(x => x.Id == entity).OnModifyAudit(userId);
            eventData.IsLocked = !eventData.IsLocked;
            this._context.Event.Update(eventData);
            return this._context.SaveChangesAsync(cancellationToken);
        }
    }
}
