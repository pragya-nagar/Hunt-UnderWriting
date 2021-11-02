using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class EventsAssignmentsMetadataQuery : CollectionQuery<IEnumerable<Guid>, EventAssignmentsMetadataModel>
    {
        private readonly Guid _systemUser = new Guid("00000000-0000-0000-0000-000000000001");
        private readonly ISynergyContext _context;

        public EventsAssignmentsMetadataQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<EventAssignmentsMetadataModel>> ExecuteAsync(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default)
        {
            var eventList = await this._context.Set<Event>().Where(x => eventIds.Contains(x.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);

            var levelPlainList = await (from level in this._context.Set<EventDecisionLevel>()
                                        join decision in this._context.Set<Decision>() on level.Id equals decision.EventDecisionLevelId into decisionLeft
                                        from decision in decisionLeft.DefaultIfEmpty()
                                        join autoDecision in this._context.Set<EventDataCutDecision>().Where(x => x.EventDataCutStrategy.IsActive) on decision.DelinquencyId equals autoDecision.DelinquencyId into autoDecisionLeft
                                        from autoDecision in autoDecisionLeft.DefaultIfEmpty()
                                        where eventIds.Contains(level.EventId)
                                        select new
                                        {
                                            level.EventId,
                                            level.Order,
                                            level.IsFinal,
                                            UserId = (Guid?)decision.UserId,
                                            DecisionDate = autoDecision != null ? autoDecision.ModifiedOn : (decision != null && decision.DecisionTypeId != null ? decision.ModifiedOn : (DateTime?)null),
                                        }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var levels = from level in levelPlainList
                         group new { level.UserId, level.DecisionDate } by new { level.Order, level.IsFinal, level.EventId } into g
                         select new
                         {
                             g.Key.EventId,
                             g.Key.Order,
                             g.Key.IsFinal,
                             Users = g.Where(x => x.UserId != null && x.UserId != this._systemUser).GroupBy(x => x.UserId.Value, x => x.DecisionDate).Select(x => (x.Key, x.All(y => y.HasValue) ? x.Max(y => y) : null)),
                         };

            var users = await (from user in this._context.Set<EventUser>()
                               where eventIds.Contains(user.EventId)
                               select new
                               {
                                   user.EventId,
                                   user.UserId,
                                   user.DepartmentId,
                               }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var manualDelinquencyList = await (from delinquency in this._context.Set<Delinquency>()
                                               join autoDecision in this._context.Set<EventDataCutDecision>().Where(x => x.EventDataCutStrategy.IsActive) on delinquency.Id equals autoDecision.DelinquencyId into autoDecisionLeft
                                               from autoDecision in autoDecisionLeft.DefaultIfEmpty()
                                               where eventIds.Contains(delinquency.EventId.Value) && delinquency.DeletedOn == null && autoDecision == null
                                               select new
                                               {
                                                   EventId = delinquency.EventId.Value,
                                                   DelinquencyId = delinquency.Id,
                                               }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<EventAssignmentsMetadataModel>();
            foreach (var @event in eventList)
            {
                result.Add(new EventAssignmentsMetadataModel
                {
                    Id = @event.Id,
                    Number = @event.EventNumber,
                    SaleDate = @event.SaleDate,
                    FundingDate = @event.FundingDate,
                    StateId = @event.StateId,
                    TypeId = @event.EventTypeId,
                    IsLocked = @event.IsLocked,
                    ManualDelinquencyCount = manualDelinquencyList.Where(x => x.EventId == @event.Id).Count(),
                    NLevelUsers = levels.Where(x => x.EventId == @event.Id && x.IsFinal == false).OrderBy(x => x.Order).Select((x, i) => (i + 1, x.Order, x.Users)),
                    FinalLevelUsers = levels.Where(x => x.EventId == @event.Id && x.IsFinal == true).SelectMany(x => x.Users),
                    DepartmentUserIds = users.Where(x => x.EventId == @event.Id).GroupBy(x => x.DepartmentId).ToDictionary(x => x.Key, x => x.Select(y => y.UserId)),
                });
            }

            return result;
        }
    }
}