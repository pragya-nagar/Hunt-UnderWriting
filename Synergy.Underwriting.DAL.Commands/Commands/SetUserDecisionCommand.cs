using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class SetUserDecisionCommand : ISetUserDecisionCommand
    {
        private ISynergyContext _context;

        public SetUserDecisionCommand(ISynergyContext context)
        {
            _context = context;
        }

        public void Dispatch(SetUserDecisionModel entity, Guid userId)
        {
            Update(entity, userId);
            _context.SaveChanges();
        }

        public Task<int> DispatchAsync(SetUserDecisionModel entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Update(entity, userId);
            return _context.SaveChangesAsync(cancellationToken);
        }

        private void Update(SetUserDecisionModel entity, Guid userId)
        {
            var decision = _context.Decision.Single(d => d.Id == entity.DecisionId);
            decision.DecisionTypeId = (int)entity.Decision;
            decision.Comment = entity.Comment;
            decision.DecisionDate = entity.DecisionDate;
            decision.OnModifyAudit(userId);

            _context.Decision.Update(decision);
        }
    }
}
