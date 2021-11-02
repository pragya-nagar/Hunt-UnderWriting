using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class UnderwritingWorkflowDefinitionQuery : SingleQuery<(int stateId, int eventTypeId), Guid?>
    {
        private readonly ISynergyContext _context;

        public UnderwritingWorkflowDefinitionQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<Guid?> ExecuteAsync((int stateId, int eventTypeId) args, CancellationToken cancellationToken = default)
        {
            var (stateId, eventTypeId) = args;
            var definitions = await (from def in this._context.Set<UnderwritingWorkflowDefinition>()
                                     join state in this._context.Set<UnderwritingWorkflowDefinitionState>() on def.Id equals state.WorkflowDefinitionId into stateLeft
                                     from state in stateLeft.DefaultIfEmpty()
                                     join type in this._context.Set<UnderwritingWorkflowDefinitionEventType>() on def.Id equals type.WorkflowDefinitionId into typeLeft
                                     from type in typeLeft.DefaultIfEmpty()
                                     where def.IsActive == true && def.DeletedOn == null && def.OriginalId == null
                                     where (state != null && type != null && state.StateId == stateId && type.EventTypeId == eventTypeId)
                                           || (state == null && type != null && type.EventTypeId == eventTypeId)
                                           || (type == null && state != null && state.StateId == stateId)
                                           || (type == null && state == null)
                                     select new
                                     {
                                         def.Id,
                                         StateId = state == null ? (int?)null : state.StateId,
                                         EventTypeId = type == null ? (int?)null : type.EventTypeId,
                                     }).ToListAsync(cancellationToken).ConfigureAwait(false);

            // if SingleOrDefault will crash a save logic works wrong
            var definition = definitions.SingleOrDefault(x => x.EventTypeId == eventTypeId && x.StateId == stateId)
                ?? definitions.SingleOrDefault(x => x.EventTypeId == null && x.StateId == stateId)
                ?? definitions.SingleOrDefault(x => x.EventTypeId == eventTypeId && x.StateId == null)
                ?? definitions.SingleOrDefault(x => x.EventTypeId == null && x.StateId == null);

            return definition?.Id;
        }
    }
}
