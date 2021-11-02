using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            this._historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        [Route("{id:guid}")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<HistoryModel>), 200)]
        public async Task<IActionResult> Get([FromRoute]Guid id, [FromQuery]FilterModel fm, CancellationToken cancellationToken = default)
        {
            var res = await this._historyService.GetListAsync(id, fm, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }
    }
}