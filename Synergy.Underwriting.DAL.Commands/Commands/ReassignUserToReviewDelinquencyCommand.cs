using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class ReassignUserToReviewDelinquencyCommand : IReassignUserToReviewDelinquencyCommand
    {
        private readonly ISynergyContext _context;

        public ReassignUserToReviewDelinquencyCommand(ISynergyContext context)
        {
            this._context = context;
        }

        public void Dispatch(IEnumerable<ReassignUsersModel> model, Guid userId)
        {
            this.DispatchAsync(model, userId).Wait();
        }

        public async Task<int> DispatchAsync(IEnumerable<ReassignUsersModel> model, Guid userId, CancellationToken cancellationToken = default)
        {
            var levels = model.Select(x => x.LevelId).ToList();

            var decisions = await _context.Decision.Where(x => levels.Contains(x.EventDecisionLevelId) && x.DeletedOn == null).ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var m in model)
            {
                _ = decisions.Join(m.Assignments, decision => decision.Id, assignment => assignment.decisionId, (decision, assignment) =>
                {
                    decision.UserId = assignment.userId;
                    decision.OnModifyAudit(userId);

                    return decision;
                }).ToList();    // ToList is important for materialize
            }

            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
