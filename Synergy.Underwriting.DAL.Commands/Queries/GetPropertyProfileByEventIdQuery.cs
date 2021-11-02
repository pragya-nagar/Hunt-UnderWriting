using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetPropertyProfileByEventIdQuery : CollectionQuery<Guid, PropertyProfile>
    {
        private readonly ISynergyContext _context;

        public GetPropertyProfileByEventIdQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<PropertyProfile>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var stateId = (await this._context.Event
                .SingleAsync(x => x.Id == eventId, cancellationToken).ConfigureAwait(false)).StateId;

            return await _context.PropertyProfile.AsNoTracking()
                .Include(x => x.PropertyProfileRulePropertyProfiles)
                .ThenInclude(x => x.PropertyProfileRule).Where(x => x.PropertyProfileStates
                .Any(p => p.StateId == stateId) && x.DeletedOn == null)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}