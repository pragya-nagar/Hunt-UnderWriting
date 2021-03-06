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
    public class GetBidByNumberQuery : SingleQuery<(Guid EventId, string Number), BidModel>
    {
        private readonly ISynergyContext _context;

        public GetBidByNumberQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<BidModel> ExecuteAsync((Guid EventId, string Number) args, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _context.Bid.Where(x => x.EventId == args.EventId && args.Number == x.Number && x.DeletedOn == null)
                .Select(x => new BidModel
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    Entity = x.Entity,
                    Number = x.Number,
                    Portfolio = x.Portfolio,
                }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}