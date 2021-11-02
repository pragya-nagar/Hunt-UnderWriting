using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class DeletePropertyProfileDelinquencyCommand : IDeletePropertyProfileDelinquencyCommand
    {
        private readonly ISynergyContext _context;

        public DeletePropertyProfileDelinquencyCommand(ISynergyContext context)
        {
            this._context = context;
        }

        public void Dispatch(DeleteDelinquencyPropertyProfileModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(DeleteDelinquencyPropertyProfileModel entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var delinquencies = await this._context.PropertyProfileDelinquency
                .Where(x => x.PropertyProfileId == entity.ProfileId
                            && x.Delinquency.EventId == entity.EventId
                            && (!entity.DelinquencyIds.Any() || entity.DelinquencyIds.Contains(x.DelinquencyId))
                            && x.Delinquency.Decisions.Any() == false && x.Delinquency.Event.IsLocked == false)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            this._context.PropertyProfileDelinquency.RemoveRange(delinquencies);

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
