using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Models.Bid;

namespace Synergy.Underwriting.Domain.Abstracts
{
    public interface IBidService
    {
        Task<SearchResultModel<BidModel>> GetListAsync(SearchArgsModel<BidFilterArgs, BidSortField> args, CancellationToken cancellationToken = default(CancellationToken));

        Task<BidDetailsModel> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    }
}
