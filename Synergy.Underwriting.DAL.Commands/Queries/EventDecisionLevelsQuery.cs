using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class EventDecisionLevelsQuery : CollectionQuery<Guid, DecisionLevelModel>
    {
        private readonly ISynergyContext _context;

        public EventDecisionLevelsQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<DecisionLevelModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.EventDecisionLevel
                .Where(x => x.EventId == eventId && x.DeletedOn == null)
                .Select(x => new DecisionLevelModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Order = x.Order,
                    IsFinal = x.IsFinal,
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}