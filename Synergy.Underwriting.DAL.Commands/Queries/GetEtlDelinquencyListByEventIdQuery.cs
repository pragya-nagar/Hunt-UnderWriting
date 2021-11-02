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
    public class GetEtlDelinquencyListByEventIdQuery : CollectionQuery<Guid, DelinquencyCalculationModel>
    {
        private readonly ISynergyContext _context;

        public GetEtlDelinquencyListByEventIdQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<DelinquencyCalculationModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var delinquencyList =
                from dl in this._context.EtlDelinquency
                where dl.EventId == eventId && this._context.Decision.Where(x => x.DeletedOn == null).All(x => x.DelinquencyId != dl.DelinquencyId)
                select new DelinquencyCalculationModel
                {
                    Id = dl.Delinquency.Id,
                    AssessedValue = dl.Delinquency.Property.PropertyValuations.OrderByDescending(x => x.AppraisedYear).Select(x => x.AppraisedValue).FirstOrDefault(),
                    GeneralLandUseCodeId = dl.Delinquency.Property.GeneralLandUseCodeId,
                    InternalLandUseCodeId = dl.Delinquency.Property.InternalLandUseCodeId,
                    LTVPercent = dl.Delinquency.LTVPercent,
                    LandUseCode = dl.Delinquency.Property.LandUseCode,
                    RULTVPercent = dl.Delinquency.RULTVPercent,
                    TotalAmountDue = dl.Delinquency.Amount,
                };
            return await delinquencyList.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
