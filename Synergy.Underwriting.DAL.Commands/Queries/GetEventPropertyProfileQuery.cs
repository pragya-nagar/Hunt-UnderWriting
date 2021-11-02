using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetEventPropertyProfileQuery : CollectionQuery<DateTime, EventPropertyProfileModel>
    {
        private readonly ISynergyContext _context;

        public GetEventPropertyProfileQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<EventPropertyProfileModel>> ExecuteAsync(DateTime dateTime, CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventState = await this._context.EtlDelinquency
                .Select(x => new
                {
                    stateId = x.Event.StateId,
                    eventId = x.Event.Id,
                }).Distinct().ToListAsync(cancellationToken).ConfigureAwait(false);

            if (eventState.Any() == false)
            {
                return Enumerable.Empty<EventPropertyProfileModel>();
            }

            List<EventPropertyProfileModel> eventPropertyList = new List<EventPropertyProfileModel>();

            foreach (var item in eventState)
            {
                var profileIds = await this._context.PropertyProfile
                    .Where(x => x.PropertyProfileStates.Any(s => s.StateId == item.stateId)).Select(x => x.Id)
                    .ToListAsync(cancellationToken).ConfigureAwait(false);

                eventPropertyList.Add(new EventPropertyProfileModel
                {
                    EventId = item.eventId,
                    PropertyProfileIds = profileIds,
                });
            }

            return eventPropertyList;
        }
    }
}
