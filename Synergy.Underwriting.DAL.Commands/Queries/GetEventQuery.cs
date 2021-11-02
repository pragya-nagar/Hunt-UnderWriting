using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetEventQuery : SingleQuery<Guid, EventModel>
    {
        private readonly ISynergyContext _context;

        public GetEventQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<EventModel> ExecuteAsync(Guid args, CancellationToken cancellationToken = default)
        {
            return await this._context.Event
                .Where(x => x.DeletedOn == null)
                .Select(x => new EventModel
                {
                    Id = x.Id,
                    CountyId = x.CountyId,
                    EventTypeId = x.EventTypeId,
                    StateId = x.StateId,
                    EventNumber = x.EventNumber,
                    SaleDate = x.SaleDate,
                })
                .FirstOrDefaultAsync(x => x.Id == args, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}