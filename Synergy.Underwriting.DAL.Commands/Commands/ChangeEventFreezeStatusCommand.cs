using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class ChangeEventFreezeStatusCommand : IChangeEventFreezeStatusCommand
    {
        private IMapper _mapper;
        private ISynergyContext _context;

        public ChangeEventFreezeStatusCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(FreezeEventStatusModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(FreezeEventStatusModel entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var events = await _context.Event.Where(x => entity.EventIds.Any(e => e == x.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
            events.ForEach(x =>
            {
                x.IsFreezed = entity.NeedToFreeze;
                x.OnModifyAudit(userId);
            });

            this._context.Event.UpdateRange(events);
            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
