using System;
using System.Collections.Generic;
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
    public class CreateAssignmentConfigurationCommand : ICreateAssignmentConfigurationCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreateAssignmentConfigurationCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(EventAssignmentModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(EventAssignmentModel entity, Guid userId, CancellationToken cancellationToken = default)
        {
            foreach (var level in entity.LevelAssignments)
            {
                var eventLevel = _mapper.Map<EventDecisionLevel>(level);
                eventLevel.OnCreateAudit(userId);
                eventLevel.EventId = entity.EventId;

                _context.EventDecisionLevel.Add(eventLevel);

                foreach (var assignment in level.Assignments)
                {
                    if (assignment.PropertyProfileId == null)
                    {
                        var userAssignment = _mapper.Map<List<EventDecisionLevelUser>>(assignment.UsersAssignment);
                        userAssignment.ForEach(x =>
                        {
                            x.OnCreateAudit(userId);
                            x.Id = Guid.NewGuid();
                            x.EventDecisionLevelId = level.LevelId;
                        });
                        _context.EventDecisionLevelUser.AddRange(userAssignment);
                        continue;
                    }

                    var levelPropertyProfile = _mapper.Map<EventDecisionLevelPropertyProfile>(assignment);
                    levelPropertyProfile.OnCreateAudit(userId);
                    levelPropertyProfile.Id = Guid.NewGuid();
                    levelPropertyProfile.EventId = entity.EventId;
                    levelPropertyProfile.EventDecisionLevelId = level.LevelId;

                    _context.EventDecisionLevelPropertyProfile.AddRange(levelPropertyProfile);

                    var userAssignments = _mapper.Map<List<EventDecisionLevelUser>>(assignment.UsersAssignment);
                    userAssignments.ForEach(x =>
                    {
                        x.OnCreateAudit(userId);
                        x.Id = Guid.NewGuid();
                        x.EventDecisionLevelId = level.LevelId;
                        x.EventDecisionLevelPropertyProfileId = levelPropertyProfile.Id;
                    });
                    _context.EventDecisionLevelUser.AddRange(userAssignments);
                }
            }

            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
