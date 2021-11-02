using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.PropertyProfile;
using Synergy.Underwriting.Models.Rule;

namespace Synergy.Underwriting.Domain.Abstracts
{
    public interface IDictionaryService
    {
        Task<SearchResultModel<FastEntityModel<int>>> GetEnumDictionaryAsync<TEnum>(CancellationToken cancellationToken = default(CancellationToken))
            where TEnum : Enum;

        Task<SearchResultModel<FastEntityModel<int>>> GetGeneralLandUseCodesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<FastEntityModel<int>>> GetInternalLandUseCodesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<FastEntityModel<int>>> GetDisplayStrategiesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<DataCutRuleFieldModel>> GetDataCutItemListAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<FastEntityModel<int>>> GetResultTypesListAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<FastEntityModel<Guid>>> GetEventListAsync(SearchArgsModel<EventDictionaryFilterArgs, EventDictionarySortField> args, CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<FastEntityModel<int>>> GetCountiesAsync(SearchArgsModel<CountySearchArgs, CountySortField> args, CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<PropertyProfileRuleFieldModel>> GetPropertyProfileRuleFieldsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<SearchResultModel<DepartmentUsersModel>> GetDepartmentUsersAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
