using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class DeleteEventDecisionCommand : IDeleteEventDecisionCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public DeleteEventDecisionCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(Guid levelId, Guid userId)
        {
            var decisions = this._context.Decision.Where(x => x.EventDecisionLevelId == levelId).ToList();

            foreach (var decision in decisions)
            {
                decision.OnDeleteAudit(userId);
            }

            this._context.Decision.UpdateRange(decisions);
            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(Guid levelId, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            // We shouldn't delete decisions where DecisionTypeId was set already
            var decisions = await this._context.Decision.Where(x => x.EventDecisionLevelId == levelId && x.DecisionTypeId == null).ToListAsync(cancellationToken).ConfigureAwait(false);

            this._context.Decision.RemoveRange(decisions);
            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
