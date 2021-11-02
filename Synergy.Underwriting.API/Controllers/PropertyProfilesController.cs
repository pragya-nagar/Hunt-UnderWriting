using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Abstracts;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.Security.Attributes;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands.PropertyProfile;
using Synergy.Underwriting.Models.PropertyProfile;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class PropertyProfilesController : Controller
    {
        private readonly IPropertyProfileService _propertyProfileService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPublishMessage _publisher;
        private readonly IMapper _mapper;

        public PropertyProfilesController(IPropertyProfileService propertyProfileService,
                                          ICurrentUserService currentUserService,
                                          IPublishMessage publisher,
                                          IMapper mapper)
        {
            this._propertyProfileService = propertyProfileService ?? throw new ArgumentNullException(nameof(propertyProfileService));
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<PropertyProfileModel>), 200)]
        [CheckPermission("Underwriting.PropertyProfile.Read")]
        public async Task<IActionResult> Get([FromQuery]SearchArgsModel<PropertyProfileFilterArgs, PropertyProfileSortField> args, CancellationToken cancellationToken = default)
        {
            var res = await this._propertyProfileService.GetListAsync(args, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }

        [Route("{id:guid}")]
        [HttpGet]
        [ProducesResponseType(typeof(PropertyProfileDetailsModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Underwriting.PropertyProfile.Read")]
        public async Task<IActionResult> Get([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._propertyProfileService.FindAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(item);
        }

        [HttpGet]
        [Route("rules")]
        [ProducesResponseType(typeof(SearchResultModel<PropertyProfileRuleModel>), 200)]
        [CheckPermission("Underwriting.PropertyProfile.Read")]
        public async Task<IActionResult> GetRules(CancellationToken cancellationToken = default)
        {
            var items = await this._propertyProfileService.GetRulesAsync(cancellationToken).ConfigureAwait(false);
            return this.Ok(items);
        }

        [HttpGet]
        [Route("rules/{id:guid}")]
        [ProducesResponseType(typeof(PropertyProfileRuleModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Underwriting.PropertyProfile.Read")]
        public async Task<IActionResult> GetRules([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._propertyProfileService.FindRuleAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(item);
        }

        [CheckPermission("Underwriting.PropertyProfile.Write")]
        [Route("rules")]
        [HttpPost]
        [ProducesResponseType(202)]
        public async Task<IActionResult> PostRules([FromBody]PropertyProfileRuleArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<PropertyProfileRuleCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            this._mapper.Map(args, command);
            await this._publisher.PublishAsync(command, cancellationToken);

            return this.AcceptedAtAction("Get", new { id = command.Id }, command.Id);
        }

        [CheckPermission("Underwriting.PropertyProfile.Write")]
        [HttpPost]
        [ProducesResponseType(typeof(Guid), 202)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Post([FromBody]PropertyProfileCreateArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<PropertyProfileCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.AcceptedAtAction("Get", new { id = command.Id }, command.Id);
        }

        [CheckPermission("Underwriting.PropertyProfile.Write")]
        [Route("{id:guid}")]
        [HttpPut]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Put([FromRoute]Guid id, [FromBody]PropertyProfileUpdateArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<PropertyProfileUpdateCommand>(id, this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }
    }
}