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
    public class GetBidsQuery : CollectionQuery<IEnumerable<Guid>, BidModel>
    {
        private readonly ISynergyContext _context;

        public GetBidsQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<BidModel>> ExecuteAsync(IEnumerable<Guid> args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var idsList = args.ToList();

            return await _context.Bid.Where(x => idsList.Contains(x.Id) && x.DeletedOn == null)
                .Select(x => new BidModel
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    Entity = x.Entity,
                    Number = x.Number,
                    Portfolio = x.Portfolio,
                }).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}