using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.Exceptions;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Bid;

namespace Synergy.Underwriting.Domain
{
    public class BidService : IBidService
    {
        private readonly IMapper _mapper;

        private readonly IQueryProvider<DAL.Queries.Entities.Bid> _bidQueryProvider;

        public BidService(IQueryProvider<DAL.Queries.Entities.Bid> bidQueryProvide, IMapper mapper)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            this._bidQueryProvider = bidQueryProvide ?? throw new ArgumentNullException(nameof(bidQueryProvide));
        }

        public async Task<SearchResultModel<BidModel>> GetListAsync(SearchArgsModel<BidFilterArgs, BidSortField> args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._bidQueryProvider.Query.Where(x => x.DeletedOn == null);

            if (string.IsNullOrWhiteSpace(args?.FullSearch) == false)
            {
                var val = args.FullSearch.Trim().ToLower(CultureInfo.InvariantCulture);
                query = query.Where(x => x.Number.ToLower().StartsWith(val)
                                      || x.Entity.ToLower().StartsWith(val)
                                      || x.Portfolio.ToLower().StartsWith(val));
            }

            if (args?.Filter?.EventIds?.Any() == true)
            {
                var ids = args.Filter.EventIds;
                query = query.Where(x => ids.Contains(x.EventId));
            }

            var count = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            var field = this.ResolveSortExpression(args?.SortField ?? BidSortField.Number);
            query = (args?.SortOrder ?? SortOrder.Asc) == SortOrder.Asc ? query.OrderBy(field) : query.OrderByDescending(field);

            query = query.Skip(args?.Offset ?? 0).Take(args?.Limit ?? 50);

            var list = await this._mapper.ProjectTo<BidModel>(query).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new SearchResultModel<BidModel>
            {
                TotalCount = count,
                List = list,
            };
        }

        public async Task<BidDetailsModel> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._bidQueryProvider.Query.Where(x => x.Id == id);

            var item = await this._mapper.ProjectTo<BidDetailsModel>(query).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return item ?? throw new NotFoundException();
        }

        private Expression<Func<DAL.Queries.Entities.Bid, object>> ResolveSortExpression(BidSortField field)
        {
            switch (field)
            {
                case BidSortField.Number:
                    return e => e.Number;
                case BidSortField.Entity:
                    return e => e.Entity;
                case BidSortField.Portfolio:
                    return e => e.Portfolio;
                default:
                    throw new ArgumentOutOfRangeException(nameof(field));
            }
        }
    }
}
