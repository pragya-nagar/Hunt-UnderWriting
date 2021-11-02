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
    public class UpdateEventCommand : IUpdateEventCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public UpdateEventCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(UpdateEventModel updateEntity, Guid userId)
        {
            this.DispatchAsync(updateEntity, userId).Wait();
        }

        public async Task<int> DispatchAsync(UpdateEventModel updateEntity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventEntity = _context.Event.Single(x => x.Id == updateEntity.Id).OnModifyAudit(userId);
            _mapper.Map(updateEntity, eventEntity);
            if (string.IsNullOrWhiteSpace(updateEntity.CountyName) == false && updateEntity.CountyId.HasValue == false)
            {
                var newCounty = new County { Name = updateEntity.CountyName, StateId = updateEntity.StateId }.OnCreateAudit(userId);
                _context.County.Add(newCounty);
                eventEntity.CountyId = newCounty.Id;
            }

            var entitiesToDelete = _context.EventUser.Where(x => x.EventId == updateEntity.Id && x.DeletedOn == null);
            if (entitiesToDelete.Any())
            {
                _context.EventUser.RemoveRange(entitiesToDelete);
            }

            List<EventUser> eventUsers = updateEntity.UserDepartments.Select(x => new EventUser
            {
                Id = Guid.NewGuid(),
                DepartmentId = x.DepartmentId,
                UserId = x.UserId,
                EventId = updateEntity.Id,
            }.OnCreateAudit(userId)).ToList();
            _context.EventUser.AddRange(eventUsers);

            _context.Event.Update(eventEntity);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
