using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetExportRulesQuery : CollectionQuery<Guid, ExportRulesModel>
    {
        private readonly ISynergyContext _synergyContext;

        public GetExportRulesQuery(ISynergyContext synergyContext)
        {
            this._synergyContext = synergyContext ?? throw new ArgumentNullException(nameof(synergyContext));
        }

        public override async Task<IEnumerable<ExportRulesModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var countyId = await this._synergyContext.Event.Where(x => x.Id == eventId).Select(x => x.CountyId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (countyId == 0)
            {
                return Enumerable.Empty<ExportRulesModel>();
            }

            var query = from r in this._synergyContext.DataCutRule
                join re in this._synergyContext.EventDataCutRule.Where(x => x.EventDataCutStrategy.EventId == eventId && x.EventDataCutStrategy.IsActive == true) on r.Id equals re.DataCutRuleId into left
                where r.CountyId == countyId
                orderby r.Name
                from re in left.DefaultIfEmpty()
                select new ExportRulesModel
                {
                    RuleName = r.Name,
                    Result = r.DataCutResultType.Description,
                    Checked = re != null ? "TRUE" : "FALSE",
                };

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}