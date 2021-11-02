using System;
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
    public class AddRulesToEventCommand : IAddRulesToEventCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public AddRulesToEventCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(AddRulesToEventModel addRulesToEntity, Guid userId)
        {
            this.DispatchAsync(addRulesToEntity, userId).Wait();
        }

        public async Task<int> DispatchAsync(AddRulesToEventModel addRulesToEntity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activeEventStrategyList = await this._context.EventDataCutStrategy.Where(x => x.EventId == addRulesToEntity.EventId && x.IsActive).ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var item in activeEventStrategyList)
            {
                item.IsActive = false;
            }

            var newStrategy = new EventDataCutStrategy
            {
                Id = Guid.NewGuid(),
                EventDataCutRules = addRulesToEntity.DataCutRuleIds.Select(x => new EventDataCutRule
                {
                    Id = Guid.NewGuid(),
                    DataCutRuleId = x,
                }.OnCreateAudit(userId)).ToList(),
                EventId = addRulesToEntity.EventId,
                IsActive = true,
            }.OnCreateAudit(userId);

            this._context.EventDataCutStrategy.Add(newStrategy);

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
