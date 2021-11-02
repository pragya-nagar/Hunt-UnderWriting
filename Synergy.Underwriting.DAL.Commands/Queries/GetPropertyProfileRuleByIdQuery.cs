using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;
using PropertyProfileLogicType = Synergy.Underwriting.DAL.Commands.Models.Results.PropertyProfileLogicType;
using PropertyProfileRuleField = Synergy.Underwriting.DAL.Commands.Models.Results.PropertyProfileRuleField;
using PropertyProfileRuleItem = Synergy.Underwriting.DAL.Commands.Models.Results.PropertyProfileRuleItem;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetPropertyProfileRuleByIdQuery : CollectionQuery<Guid, PropertyProfileRuleModel>
    {
        private readonly ISynergyContext _context;

        public GetPropertyProfileRuleByIdQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<PropertyProfileRuleModel>> ExecuteAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            return await this._context.PropertyProfileRule.AsNoTracking()
                .Include(x => x.PropertyProfileRuleItems)
                .ThenInclude(x => x.PropertyProfileRuleItemValues)
                .Where(x => x.PropertyProfileRulePropertyProfiles.Any(p => p.PropertyProfileId == profileId))
                .Select(rule => new PropertyProfileRuleModel
                {
                    Id = rule.Id,
                    Items = rule.PropertyProfileRuleItems.Select(item => new PropertyProfileRuleItem
                    {
                        Logic = (PropertyProfileLogicType)item.PropertyProfileLogicTypeId,
                        Field = (PropertyProfileRuleField)item.PropertyProfileRuleFieldId,
                        Values = item.PropertyProfileRuleItemValues.Select(value => value.Value).ToList(),
                    }).ToList(),
                })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}