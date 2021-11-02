using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class UpdateAssignmentConfigurationCommand : IUpdateAssignmentConfigurationCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public UpdateAssignmentConfigurationCommand(ISynergyContext context, IMapper mapper)
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
            var existingLevels = _context.EventDecisionLevel.Include(x => x.EventDecisionLevelUser).Where(x => x.EventId == entity.EventId);

            if (!entity.LevelAssignments.Any())
            {
                _context.EventDecisionLevel.RemoveRange(existingLevels);
                return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var levelsToDelete = existingLevels.Where(e => existingLevels.Select(x => x.Id).Except(entity.LevelAssignments.Select(x => x.LevelId)).Any(a => a == e.Id));
            if (levelsToDelete.Any())
            {
                _context.EventDecisionLevel.RemoveRange(levelsToDelete);
            }

            foreach (var level in entity.LevelAssignments)
            {
                var levelExists = existingLevels.Where(x => x.Id == level.LevelId).FirstOrDefault();
                AddOrUpdateExistingEntities(level, levelExists, userId, entity.EventId);
            }

            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private void AddOrUpdateExistingEntities(LevelAssignmentModel level, EventDecisionLevel existingLevel, Guid userId, Guid eventId)
        {
            Guid levelId;
            if (existingLevel == null)
            {
                var eventLevel = this._mapper.Map<EventDecisionLevel>(level);
                levelId = eventLevel.Id;
                eventLevel.OnCreateAudit(userId);
                eventLevel.EventId = eventId;
                _context.EventDecisionLevel.Add(eventLevel);
            }
            else
            {
                this._mapper.Map(level, existingLevel);
                existingLevel.OnModifyAudit(userId);
                _context.EventDecisionLevel.Update(existingLevel);

                var existingLevelProfiles = _context.EventDecisionLevelPropertyProfile.Where(x => x.EventDecisionLevelId == level.LevelId);
                _context.EventDecisionLevelPropertyProfile.RemoveRange(existingLevelProfiles);

                var existingLevelUsers = existingLevel.EventDecisionLevelUser;
                _context.EventDecisionLevelUser.RemoveRange(existingLevelUsers);

                levelId = existingLevel.Id;
            }

            foreach (var assignment in level.Assignments)
            {
                if (!assignment.PropertyProfileId.HasValue)
                {
                    var entities = _mapper.Map<List<EventDecisionLevelUser>>(assignment.UsersAssignment);
                    entities.ForEach(x =>
                    {
                        x.OnCreateAudit(userId);
                        x.Id = Guid.NewGuid();
                        x.EventDecisionLevelId = levelId;
                        x.EventDecisionLevelPropertyProfileId = null;
                    });
                    _context.EventDecisionLevelUser.AddRange(entities);
                    continue;
                }

                AddOrUpdateEventDecisionLevelPropertyProfiles(assignment, userId, eventId, levelId);
            }
        }

        private void AddOrUpdateEventDecisionLevelPropertyProfiles(PropertyProfileLevelAssignmentModel assignment, Guid userId, Guid eventId, Guid levelId)
        {
            var levelPropertyProfile = _mapper.Map<EventDecisionLevelPropertyProfile>(assignment);
            levelPropertyProfile.OnCreateAudit(userId);
            levelPropertyProfile.Id = Guid.NewGuid();
            levelPropertyProfile.EventId = eventId;
            levelPropertyProfile.EventDecisionLevelId = levelId;

            _context.EventDecisionLevelPropertyProfile.Add(levelPropertyProfile);

            var entities = _mapper.Map<List<EventDecisionLevelUser>>(assignment.UsersAssignment);
            entities.ForEach(x =>
            {
                x.OnCreateAudit(userId);
                x.Id = Guid.NewGuid();
                x.EventDecisionLevelId = levelId;
                x.EventDecisionLevelPropertyProfileId = levelPropertyProfile.Id;
            });
            _context.EventDecisionLevelUser.AddRange(entities);
        }
    }
}
