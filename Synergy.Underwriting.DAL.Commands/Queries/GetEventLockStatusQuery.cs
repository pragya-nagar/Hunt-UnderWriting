using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetEventLockStatusQuery : SingleQuery<Guid, bool>
    {
        private readonly ISynergyContext _context;

        public GetEventLockStatusQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<bool> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._context.Event
                .Where(x => x.Id == eventId)
                .Select(x => x.IsLocked)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}