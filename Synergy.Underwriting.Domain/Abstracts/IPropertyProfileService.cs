using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.PropertyProfile;

namespace Synergy.Underwriting.Domain.Abstracts
{
    public interface IPropertyProfileService
    {
        Task<SearchResultModel<PropertyProfileRuleModel>> GetRulesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<PropertyProfileModel>> GetListAsync(SearchArgsModel<PropertyProfileFilterArgs, PropertyProfileSortField> args, CancellationToken cancellationToken);

        Task<PropertyProfileDetailsModel> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<PropertyProfileRuleModel> FindRuleAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    }
}
