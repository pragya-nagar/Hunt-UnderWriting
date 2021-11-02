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
    public class DelinquencyDecisionsQuery : CollectionQuery<Guid, DecisionModel>
    {
        private readonly ISynergyContext _context;

        public DelinquencyDecisionsQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<DecisionModel>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await this._context.Decision.Where(x => x.DelinquencyId == id && x.DeletedOn == null)
                .Select(x => new DecisionModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    DecisionTypeId = x.DecisionTypeId,
                    LevelId = x.EventDecisionLevel.Id,
                    IsFinal = x.EventDecisionLevel.IsFinal,
                    Order = x.EventDecisionLevel.Order,
                }).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
