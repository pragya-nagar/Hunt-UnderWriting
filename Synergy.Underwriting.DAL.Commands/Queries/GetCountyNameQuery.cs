using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetCountyNameQuery : SingleQuery<int, string>
    {
        private readonly ISynergyContext _context;

        public GetCountyNameQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<string> ExecuteAsync(int countyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._context.County
                                      .Where(s => s.Id == countyId)
                                      .Select(x => x.Name)
                                      .FirstOrDefaultAsync(cancellationToken)
                                      .ConfigureAwait(false);
        }
    }
}
