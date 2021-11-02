using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetStateAbbreviationQuery : SingleQuery<int, string>
    {
        private readonly ISynergyContext _context;

        public GetStateAbbreviationQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<string> ExecuteAsync(int stateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._context.State
                .Where(s => s.Id == stateId)
                .Select(x => x.Abbreviation)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}