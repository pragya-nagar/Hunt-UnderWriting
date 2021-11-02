using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class CheckLevelReviewFinishedQuery : SingleQuery<(Guid LevelId, Guid UserId), bool>
    {
        private readonly ISynergyContext _context;

        public CheckLevelReviewFinishedQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<bool> ExecuteAsync((Guid LevelId, Guid UserId) args, CancellationToken cancellationToken = default)
        {
            var (levelId, userId) = args;

            var eventDataCutDecisions = this._context.EventDataCutDecision.Where(x => x.EventDataCutStrategy.IsActive);

            var hasNotReviewed = await this._context.Decision
                             .GroupJoin(eventDataCutDecisions, x => x.DelinquencyId, y => y.DelinquencyId, (d, ad) => new { Decision = d, AutoDecision = ad })
                             .AnyAsync(x => x.Decision.UserId == userId
                                         && x.Decision.EventDecisionLevelId == levelId
                                         && x.Decision.DeletedOn == null
                                         && x.Decision.DecisionTypeId == null
                                         && x.AutoDecision.Any() == false, cancellationToken)
                             .ConfigureAwait(false);

            return hasNotReviewed == false;
        }
    }
}