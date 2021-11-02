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
    public class AttachFileToEventCommand : IAttachFileToEventCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public AttachFileToEventCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispatch(AttachFileToEventModel entity, Guid userId)
        {
            var data = _mapper.Map<EventAttachment>(entity).OnCreateAudit(userId);

            _context.EventAttachment.Add(data);
            _context.SaveChanges();
        }

        public Task<int> DispatchAsync(AttachFileToEventModel entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = _mapper.Map<EventAttachment>(entity).OnCreateAudit(userId);
            _context.EventAttachment.Add(data);
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
