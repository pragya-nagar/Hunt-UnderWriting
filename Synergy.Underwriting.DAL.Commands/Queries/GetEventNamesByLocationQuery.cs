using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetEventNamesByLocationQuery : CollectionQuery<(int StateId, string CountyName, int EventTypeId, int SaleYear), string>
    {
        private readonly ISynergyContext _context;

        public GetEventNamesByLocationQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<string>> ExecuteAsync((int StateId, string CountyName, int EventTypeId, int SaleYear) args, CancellationToken cancellationToken = default)
        {
            return await this._context.Event.Where(e => e.StateId == args.StateId
                                                     && e.County.Name == args.CountyName
                                                     && e.SaleDate.Year == args.SaleYear
                                                     && e.EventTypeId == args.EventTypeId)
                                            .Select(x => x.EventNumber)
                                            .ToListAsync(cancellationToken)
                                            .ConfigureAwait(false);
        }
    }
}