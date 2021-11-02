using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class CreateEventDataCutDecisionsCommand : ICreateEventDataCutDecisionsCommand
    {
        private IMapper _mapper;
        private ISynergyContext _context;

        public CreateEventDataCutDecisionsCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(IEnumerable<CreateEventDataCutDecisionModel> entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(IEnumerable<CreateEventDataCutDecisionModel> entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = _mapper.Map<IEnumerable<EventDataCutDecision>>(entity).ToList();
            data.ForEach(e =>
            {
                e.OnCreateAudit(userId);
            });
            _context.EventDataCutDecision.AddRange(data);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
