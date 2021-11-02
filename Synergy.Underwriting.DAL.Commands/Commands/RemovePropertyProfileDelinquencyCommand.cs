using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class RemovePropertyProfileDelinquencyCommand : IRemovePropertyProfileDelinquencyCommand
    {
        private readonly ISynergyContext _context;

        public RemovePropertyProfileDelinquencyCommand(ISynergyContext context)
        {
            this._context = context;
        }

        public void Dispatch(IEnumerable<Guid> entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(IEnumerable<Guid> entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var delinquencies = await this._context.PropertyProfileDelinquency
                .Where(x => entity.Contains(x.DelinquencyId)).ToListAsync(cancellationToken).ConfigureAwait(false);
            this._context.PropertyProfileDelinquency.RemoveRange(delinquencies);

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
