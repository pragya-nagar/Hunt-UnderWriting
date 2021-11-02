using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.PropertyProfile;
using Synergy.Underwriting.Models.Rule;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class DictionariesController : Controller
    {
        private readonly IDictionaryService _dictionaryService;

        public DictionariesController(IDictionaryService dictionaryService)
        {
            this._dictionaryService = dictionaryService;
        }

        [Route("EventTypes")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetEventTypes(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetEnumDictionaryAsync<DataAccess.Enum.EventType>(cancellationToken);
            return this.Ok(res);
        }

        [Route("AuctionTypes")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetAuctionTypes(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetEnumDictionaryAsync<DataAccess.Enum.AuctionType>(cancellationToken);
            return this.Ok(res);
        }

        [Route("SaleDateStatuses")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetSaleDateStatuses(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetEnumDictionaryAsync<DataAccess.Enum.SaleDateStatus>(cancellationToken);
            return this.Ok(res);
        }

        [Route("FinalPaymentTypes")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetFinalPaymentTypes(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetEnumDictionaryAsync<DataAccess.Enum.FinalPaymentType>(cancellationToken);
            return this.Ok(res);
        }

        [Route("EntityTypes")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetEntityTypes(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetEnumDictionaryAsync<DataAccess.Enum.EventEntity>(cancellationToken);
            return this.Ok(res);
        }

        [Route("RuleFields")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<DataCutRuleFieldModel>), 200)]
        public async Task<IActionResult> GetRuleFields(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetDataCutItemListAsync(cancellationToken);
            return this.Ok(res);
        }

        [Route("DisplayStrategies")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetDisplayStrategies(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetDisplayStrategiesAsync(cancellationToken);
            return this.Ok(res);
        }

        [Route("ResultTypes")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetResultTypes(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetResultTypesListAsync(cancellationToken);
            return this.Ok(res);
        }

        [Route("GeneralLandUseCodes")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetGeneralLandUseCodes(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetGeneralLandUseCodesAsync(cancellationToken);
            return this.Ok(res);
        }

        [Route("InternalLandUseCodes")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetInternalLandUseCodes(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetInternalLandUseCodesAsync(cancellationToken);
            return this.Ok(res);
        }

        [Route("events")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetEvents([FromQuery]SearchArgsModel<EventDictionaryFilterArgs, EventDictionarySortField> args, CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetEventListAsync(args, cancellationToken);
            return this.Ok(res);
        }

        [Route("counties")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<FastEntityModel<int>>), 200)]
        public async Task<IActionResult> GetCounties([FromQuery]SearchArgsModel<CountySearchArgs, CountySortField> args, CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetCountiesAsync(args, cancellationToken);
            return this.Ok(res);
        }

        [Route("PropertyProfileRuleFields")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<PropertyProfileRuleFieldModel>), 200)]
        public async Task<IActionResult> GetPropertyProfileRuleFields(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetPropertyProfileRuleFieldsAsync(cancellationToken);
            return this.Ok(res);
        }

        [Route("DepartmentUsers")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<DepartmentUsersModel>), 200)]
        public async Task<IActionResult> GetDepartmentUsers(CancellationToken cancellationToken = default)
        {
            var res = await this._dictionaryService.GetDepartmentUsersAsync(cancellationToken);
            return this.Ok(res);
        }
    }
}