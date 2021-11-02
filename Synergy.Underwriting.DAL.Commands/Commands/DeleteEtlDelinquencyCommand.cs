using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class DeleteEtlDelinquencyCommand : IDeleteEtlDelinquencyCommand
    {
        private readonly ISynergyContext _context;

        public DeleteEtlDelinquencyCommand(ISynergyContext context)
        {
            this._context = context;
        }

        public void Dispatch(Guid guid, Guid userId)
        {
            this.DispatchAsync(guid, userId).Wait();
        }

        public async Task<int> DispatchAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            List<DataAccess.Entities.EtlDelinquency> all = _context.EtlDelinquency.Where(etld => etld.EventId == eventId).ToList();
            _context.EtlDelinquency.RemoveRange(all);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
