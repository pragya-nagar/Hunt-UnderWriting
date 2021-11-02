using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetPropertyProfileByIdQuery : SingleQuery<Guid, PropertyProfileModel>
    {
        private readonly ISynergyContext _context;

        public GetPropertyProfileByIdQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<PropertyProfileModel> ExecuteAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            return await this._context.PropertyProfile.AsNoTracking().Where(x => x.Id == profileId && x.DeletedOn == null)
                .Select(x => new PropertyProfileModel
                {
                    Id = x.Id,
                    IsActive = x.IsActive,
                    Name = x.Name,
                    StateIds = x.PropertyProfileStates.Select(s => s.StateId).ToList(),
                    RuleIds = x.PropertyProfileRulePropertyProfiles.Select(r => r.PropertyProfileRuleId).ToList(),
                }).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}