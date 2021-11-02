using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.Domain.Abstracts
{
    public interface IHistoryService
    {
        Task<SearchResultModel<HistoryModel>> GetListAsync(Guid id, FilterModel fm, CancellationToken cancellationToken = default(CancellationToken));
    }
}
