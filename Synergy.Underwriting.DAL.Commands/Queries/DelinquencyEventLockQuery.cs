using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class DelinquencyEventLockQuery : SingleQuery<Guid, (Guid EventId, bool IsLocked, bool IsRejectReasonReuired)>
    {
        private readonly ISynergyContext _context;

        public DelinquencyEventLockQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<(Guid EventId, bool IsLocked, bool IsRejectReasonReuired)> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var evt = await _context.Delinquency.Where(x => x.Id == id).Select(x => new { x.Event.Id, x.Event.IsLocked, x.Event.IsRejectReasonRequired }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            return (evt.Id, evt.IsLocked, evt.IsRejectReasonRequired);
        }
    }
}