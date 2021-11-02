using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class CheckProfileNameStatesQuery : SingleQuery<(string ProfileName, IEnumerable<int> StateIds, Guid ProfileId), bool>
    {
        private readonly ISynergyContext _context;

        public CheckProfileNameStatesQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<bool> ExecuteAsync((string ProfileName, IEnumerable<int> StateIds, Guid ProfileId) args, CancellationToken cancellationToken = default)
        {
            var (profileName, stateIds, profileId) = args;

            var profileExist = await this._context.PropertyProfile
                .AnyAsync(x =>
                    x.Id != profileId &&
                    x.Name.ToLower() == profileName.ToLower() &&
                    x.PropertyProfileStates.Any(p => stateIds.Contains(p.StateId)), cancellationToken)
                .ConfigureAwait(false);

            return profileExist;
        }
    }
}