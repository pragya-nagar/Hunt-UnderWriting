using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class EventDecisionQuery : CollectionQuery<Guid, LevelDecisionModel>
    {
        private readonly ISynergyContext _context;

        public EventDecisionQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<LevelDecisionModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.Decision
                .Where(x => x.EventDecisionLevel.EventId == eventId && x.DeletedOn == null)
                .Select(x => new LevelDecisionModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    DelinquencyId = x.DelinquencyId,
                    LevelId = x.EventDecisionLevelId,
                    DecisionType = (DecisionType?)x.DecisionTypeId,
                }).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}