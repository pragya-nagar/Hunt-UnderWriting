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
    public class GetBidListQuery : SingleQuery<Guid, IDictionary<string, BidModel>>
    {
        private readonly ISynergyContext _context;

        public GetBidListQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IDictionary<string, BidModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _context.Bid.Where(x => x.EventId == eventId && x.DeletedOn == null)
                .Select(x => new BidModel
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    Entity = x.Entity,
                    Number = x.Number,
                    Portfolio = x.Portfolio,
                }).ToDictionaryAsync(x => x.Number, cancellationToken).ConfigureAwait(false);
        }
    }
}
