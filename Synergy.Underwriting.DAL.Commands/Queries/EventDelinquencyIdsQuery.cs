using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class EventDelinquencyIdsQuery : CollectionQuery<Guid, Guid>
    {
        private readonly ISynergyContext _context;

        public EventDelinquencyIdsQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<Guid>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.Delinquency.Where(x => x.EventId == eventId && x.DeletedOn == null)
                .Select(x => x.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}