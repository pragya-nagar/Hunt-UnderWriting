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
    public class EventDataCutDelinquencyIdsQuery : CollectionQuery<Guid, Guid>
    {
        private readonly ISynergyContext _context;

        public EventDataCutDelinquencyIdsQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<Guid>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.EventDataCutDecision
                .Where(x => x.EventDataCutStrategy.EventId == eventId && x.EventDataCutStrategy.IsActive == true && x.DeletedOn == null)
                .Select(x => x.DelinquencyId)
                .Distinct()
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}