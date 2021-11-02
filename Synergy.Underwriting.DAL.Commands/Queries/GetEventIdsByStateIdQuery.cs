using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetEventIdsByStateIdQuery : CollectionQuery<IEnumerable<int>, EventStateModel>
    {
        private readonly ISynergyContext _context;

        public GetEventIdsByStateIdQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<EventStateModel>> ExecuteAsync(IEnumerable<int> stateIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._context.Event.AsNoTracking().Where(x => stateIds.Contains(x.StateId) && x.DeletedOn == null && x.IsLocked == false)
                .Select(x => new EventStateModel { EventId = x.Id, StateId = x.StateId }).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
