using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    using Task = System.Threading.Tasks.Task;

    public class UpdatePropertyCommand : IUpdatePropertyCommand
    {
        private readonly ISynergyContext _context;

        public UpdatePropertyCommand(ISynergyContext context)
        {
            _context = context;
        }

        public void Dispatch(UpdatePropertyModel entity, Guid userId)
        {
            Property property = _context.Property.Single(p => p.Delinquencies.Any(d => d.Id == entity.Id));
            UpdateProperty(entity, property, userId).Wait();

            _context.SaveChanges();
        }

        public async Task<int> DispatchAsync(UpdatePropertyModel entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Property property = await _context.Property.SingleAsync(p => p.Delinquencies.Any(d => d.Id == entity.Id), cancellationToken).ConfigureAwait(false);
            await UpdateProperty(entity, property, userId).ConfigureAwait(false);

            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateProperty(UpdatePropertyModel updateEntity, Property property, Guid userId)
        {
            property.InternalLandUseCodeId = updateEntity.InternalLandUseCodeId;
            property.GeneralLandUseCodeId = updateEntity.GeneralLandUseCodeId;
            property.Homestead = updateEntity.IsHomestead;

            _context.Property.Update(property.OnModifyAudit(userId));

            await UpdateDisplayStrategies(updateEntity, userId).ConfigureAwait(false);

            UpdateScoring(updateEntity, userId);
        }

        private async Task UpdateDisplayStrategies(UpdatePropertyModel updateEntity, Guid userId)
        {
            var displayStrategies = _context.DelinquencyPropertyDisplayStrategy.Where(x => x.DelinquencyId == updateEntity.Id);

            var currentDsiplayStrategies = await displayStrategies.Select(x => x.PropertyDisplayStrategyId).ToListAsync().ConfigureAwait(false);

            if (currentDsiplayStrategies.OrderBy(x => x).SequenceEqual(updateEntity.DispStrategyIds.OrderBy(x => x)) == true)
            {
                return;
            }

            if (displayStrategies.Any())
            {
                _context.DelinquencyPropertyDisplayStrategy.RemoveRange(displayStrategies);
            }

            if (updateEntity.DispStrategyIds.Any())
            {
                var addDispStrategies = new List<DelinquencyPropertyDisplayStrategy>();
                foreach (var id in updateEntity.DispStrategyIds)
                {
                    addDispStrategies.Add(new DelinquencyPropertyDisplayStrategy
                    {
                        Id = Guid.NewGuid(),
                        DelinquencyId = updateEntity.Id,
                        PropertyDisplayStrategyId = id,
                    }.OnCreateAudit(userId));
                }

                _context.DelinquencyPropertyDisplayStrategy.AddRange(addDispStrategies);
            }
        }

        private void UpdateScoring(UpdatePropertyModel updateEntity, Guid userId)
        {
            var scoring = _context.DelinquencyPropertyScoring.SingleOrDefault(x => x.DelinquencyId == updateEntity.Id);

            if (scoring == null)
            {
                _context.DelinquencyPropertyScoring.Add(new DelinquencyPropertyScoring
                {
                    Id = Guid.NewGuid(),
                    DelinquencyId = updateEntity.Id,
                    PropertyScoring = updateEntity.PropertyScoring,
                }.OnCreateAudit(userId));
                return;
            }

            if (updateEntity.PropertyScoring.HasValue)
            {
                scoring.PropertyScoring = updateEntity.PropertyScoring;
                _context.DelinquencyPropertyScoring.Update(scoring.OnModifyAudit(userId));
                return;
            }

            _context.DelinquencyPropertyScoring.Remove(scoring);
        }
    }
}
