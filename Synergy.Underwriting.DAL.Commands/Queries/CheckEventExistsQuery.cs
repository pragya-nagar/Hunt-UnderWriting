using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class CheckEventExistsQuery : SingleQuery<Guid, bool>
    {
        private readonly ISynergyContext _context;

        public CheckEventExistsQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<bool> ExecuteAsync(Guid args, CancellationToken cancellationToken = default)
        {
            return await this._context.Event.AnyAsync(x => x.Id == args && x.DeletedOn == null, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}