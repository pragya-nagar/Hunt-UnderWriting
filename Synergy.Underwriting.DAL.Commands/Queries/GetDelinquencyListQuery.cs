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
    public class GetDelinquencyListQuery : CollectionQuery<Guid, DelinquencyModel>
    {
        private readonly ISynergyContext _context;

        public GetDelinquencyListQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<DelinquencyModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var map = await _context.Delinquency.Where(x => x.EventId == eventId && x.DeletedOn == null)
                .GroupJoin(_context.Result.Where(x => x.DeletedOn == null), x => x.Id, r => r.DelinquencyId, (d, r) => new { d, r })
                .SelectMany(x => x.r.DefaultIfEmpty(),
                (d, r) =>
                new
                {
                    d.d.Id,
                    d.d.Property.ParcelId,
                    d.d.PropertySupplementalEventData.AdvertisementNumber,
                    d.d.DelinquencyTaxYear,
                    ResultId = (Guid?)r.Id,
                    ResultCreatedOn = (DateTime?)r.CreatedOn,
                    ResultCreatedById = (Guid?)r.CreatedById,
                })
                .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return map.GroupBy(x => x.Id)
                    .Select(x =>
                {
                    var first = x.FirstOrDefault();
                    return new DelinquencyModel
                    {
                        Id = x.Key,
                        ParcelId = first?.ParcelId,
                        AdvertisementNumber = first?.AdvertisementNumber,
                        TaxYear = first?.DelinquencyTaxYear,
                        ResultId = first?.ResultId,
                        ResultCreatedById = first?.ResultCreatedById,
                        ResultCreatedOn = first?.ResultCreatedOn,
                    };
                }).ToList();
        }
    }
}
