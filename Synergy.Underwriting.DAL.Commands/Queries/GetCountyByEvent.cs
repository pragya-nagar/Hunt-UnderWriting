using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetCountyByEvent : SingleQuery<Guid, int>
    {
        private readonly ISynergyContext _context;

        public GetCountyByEvent(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<int> ExecuteAsync(Guid args, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._context.Event.AsNoTracking().Where(x => x.Id == args && x.DeletedOn == null)
                .Select(x => x.CountyId).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
