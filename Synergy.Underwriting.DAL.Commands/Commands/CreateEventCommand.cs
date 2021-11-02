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
    public class CreateEventCommand : ICreateEventCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreateEventCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(CreateEventModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(CreateEventModel entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var data = this._mapper.Map<Event>(entity).OnCreateAudit(userId);

            if (string.IsNullOrWhiteSpace(entity.CountyName) == false && entity.CountyId.HasValue == false)
            {
                var newCounty = new County { Name = entity.CountyName, StateId = entity.StateId }.OnCreateAudit(userId);
                this._context.County.Add(newCounty);
                data.CountyId = newCounty.Id;
            }

            List<EventUser> eventUsers = entity.UserDepartments.Select(x => new EventUser
            {
                Id = Guid.NewGuid(),
                DepartmentId = x.DepartmentId,
                UserId = x.UserId,
                EventId = entity.Id,
            }.OnCreateAudit(userId)).ToList();
            this._context.EventUser.AddRange(eventUsers);

            this._context.Event.Add(data);

            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
