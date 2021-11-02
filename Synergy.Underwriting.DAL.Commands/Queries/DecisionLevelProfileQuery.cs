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
    public class DecisionLevelProfileQuery : CollectionQuery<Guid, DecisionLevelProfileModel>
    {
        private readonly ISynergyContext _context;

        public DecisionLevelProfileQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<DecisionLevelProfileModel>> ExecuteAsync(Guid levelId, CancellationToken cancellationToken = default)
        {
            return await _context.EventDecisionLevelPropertyProfile
                .Where(x => x.EventDecisionLevelId == levelId && x.DeletedOn == null)
                .Select(x => new DecisionLevelProfileModel
                {
                    Id = x.EventDecisionLevelId,
                    PropertyProfileId = x.PropertyProfileId,
                    Order = x.Order,
                    EventDecisionLevelId = x.EventDecisionLevelId,
                    UsersAssignment = x.EventDecisionLevel.EventDecisionLevelUser.Where(elu => elu.EventDecisionLevelPropertyProfile.PropertyProfileId == x.PropertyProfileId)
                    .Select(u => new DecisionLevelUserAssignmentModel
                    {
                        AssignmentsCount = u.AssigmentCount,
                        UserId = u.UserId,
                    }).ToList(),
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}