using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class RefreshResultToBidRelationCommand : IRefreshResultToBidRelationCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public RefreshResultToBidRelationCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(RefreshResultToBidRelationModel model, Guid userId)
        {
            var bidQuery = this._context.Bid.Where(x => x.EventId == model.Id && x.DeletedOn == null);
            var resultQuery = this._context.Delinquency.Where(x => x.EventId == model.Id && x.DeletedOn == null).SelectMany(x => x.Results.Where(y => y.DeletedOn == null));

            var query = from r in resultQuery
                        join b in bidQuery on r.BidNumber equals b.Number into left
                        from b in left.DefaultIfEmpty()
                        where r.BidId != b.Id
                        select new
                        {
                            BidId = (Guid?)b.Id,
                            Result = r,
                        };

            var list = query.Distinct().ToList();
            foreach (var item in list)
            {
                if (item.Result.BidId != item.BidId)
                {
                    item.Result.BidId = item.BidId;
                    item.Result.OnModifyAudit(userId);
                }
            }

            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(RefreshResultToBidRelationModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var bidQuery = this._context.Bid.Where(x => x.EventId == model.Id && x.DeletedOn == null);

            var resultQuery = this._context.Delinquency
                                           .Where(x => x.EventId == model.Id && x.DeletedOn == null)
                                           .SelectMany(x => x.Results.Where(y => y.DeletedOn == null));

            var query = from r in resultQuery
                        join b in bidQuery on r.BidNumber equals b.Number into left
                        from b in left.DefaultIfEmpty()
                        where r.BidId != b.Id
                        select new
                        {
                            BidId = (Guid?)b.Id,
                            Result = r,
                        };

            var list = await query.Distinct().ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var item in list)
            {
                if (item.Result.BidId != item.BidId)
                {
                    item.Result.BidId = item.BidId;
                    item.Result.OnModifyAudit(userId);
                }
            }

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
