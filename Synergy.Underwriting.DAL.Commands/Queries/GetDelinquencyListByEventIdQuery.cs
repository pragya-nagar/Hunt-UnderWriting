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
    public class GetDelinquencyListByEventIdQuery : CollectionQuery<Guid, DelinquencyCalculationModel>
    {
        private readonly ISynergyContext _context;

        public GetDelinquencyListByEventIdQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<DelinquencyCalculationModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var delinquencyList =
                from dl in this._context.Delinquency
                where dl.EventId == eventId && this._context.Decision.Where(x => x.DeletedOn == null).All(x => x.DelinquencyId != dl.Id)
                select new DelinquencyCalculationModel
                {
                    Id = dl.Id,
                    AssessedValue = dl.Property.PropertyValuations.OrderByDescending(x => x.AppraisedYear).Select(x => x.AppraisedValue).FirstOrDefault(),
                    GeneralLandUseCodeId = dl.Property.GeneralLandUseCodeId,
                    InternalLandUseCodeId = dl.Property.InternalLandUseCodeId,
                    LTVPercent = dl.LTVPercent,
                    LandUseCode = dl.Property.LandUseCode,
                    RULTVPercent = dl.RULTVPercent,
                    TotalAmountDue = dl.Amount,
                };

            return await delinquencyList.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
