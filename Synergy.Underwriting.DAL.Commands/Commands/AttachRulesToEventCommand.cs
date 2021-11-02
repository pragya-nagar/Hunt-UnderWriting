using System;
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
    public class AttachRulesToEventCommand : IAttachRulesToEventCommand
    {
        private IMapper _mapper;
        private ISynergyContext _context;

        public AttachRulesToEventCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(AttachRulesToEventModel entity, Guid userId)
        {
            var data = _mapper.Map<EventDataCutStrategy>(entity).OnCreateAudit(userId);
            data.EventDataCutRules.ForEach(x =>
            {
                x.OnCreateAudit(userId);
                x.Id = Guid.NewGuid();
            });

            _context.EventDataCutStrategy.Add(data);
            _context.SaveChanges();
        }

        public Task<int> DispatchAsync(AttachRulesToEventModel entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = _mapper.Map<EventDataCutStrategy>(entity).OnCreateAudit(userId);
            data.EventDataCutRules.ForEach(x =>
            {
                x.OnCreateAudit(userId);
                x.Id = Guid.NewGuid();
            });

            _context.EventDataCutStrategy.Add(data);
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
