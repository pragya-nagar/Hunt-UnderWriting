using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Synergy.Common.Abstracts;
using Synergy.Common.Domain.Models.Common;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands.EventAssignment;
using Synergy.Underwriting.Models.PropertyProfile;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/events")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class EventAssignmentsController : Controller
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IPublishMessage _publisher;
        private readonly IMapper _mapper;

        private readonly IEventAssignmentsService _eventAssignmentsService;

        public EventAssignmentsController(ICurrentUserService currentUserService,
                                          IPublishMessage publisher,
                                          IMapper mapper,
                                          IEventAssignmentsService eventAssignmentsService)
        {
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._eventAssignmentsService = eventAssignmentsService ?? throw new ArgumentNullException(nameof(eventAssignmentsService));
        }

        [Route("{eventId:guid}/propertyProfiles")]
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<EventAssignmentProfileModel>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Get([FromRoute]Guid eventId, [FromQuery] Guid[] orderedProfiles, [FromQuery] bool isAssignmentStep, CancellationToken cancellationToken = default)
        {
            if (!isAssignmentStep)
            {
                var model = await this._eventAssignmentsService.FindAsync(eventId, cancellationToken).ConfigureAwait(false);
                return this.Ok(model);
            }

            List<PropertyProfileOrderModel> listArgs = new List<PropertyProfileOrderModel>();

            for (int i = 0; i < orderedProfiles.Count(); i++)
            {
                listArgs.Add(new PropertyProfileOrderModel { Order = i, PropertyProfileId = orderedProfiles[i] });
            }

            var args = new OrderedProfileArgs { EventId = eventId, ProfileOrders = listArgs };
            var item = await this._eventAssignmentsService.GetOrderedProfilesAsync(args, cancellationToken).ConfigureAwait(false);

            return this.Ok(item);
        }

        [HttpGet]
        [Route("{eventId:guid}/assignment")]
        [ProducesResponseType(typeof(EventAssignmentModel), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get([FromRoute]Guid eventId, CancellationToken cancellationToken = default)
        {
            var item = await this._eventAssignmentsService.GetAssignmentsAsync(eventId, cancellationToken).ConfigureAwait(false);

            return this.Ok(item);
        }

        [Route("{eventId:guid}/assignment")]
        [HttpPost]
        [ProducesResponseType(202)]
        public async Task<IActionResult> Post([FromRoute]Guid eventId, [FromBody] IEnumerable<EventAssignmentCreateArgs> args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<EventAssignmentCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = eventId;
            command.LevelAssignments = args.Select(x => new LevelAssignmentModel
            {
                Name = x.Name,
                IsFinal = x.IsFinal,
                LevelId = Guid.NewGuid(),
                Assignments = x.Assignments,
                Order = x.Order,
            });

            await this._eventAssignmentsService.CreateAssignmentAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("{eventId:guid}/assignment")]
        [HttpPut]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Put([FromRoute]Guid eventId, [FromBody] IEnumerable<EventAssignmentUpdateArgs> args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<EventAssignmentUpdateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = eventId;
            command.LevelAssignments = args.Select(x => new LevelAssignmentModel
            {
                Name = x.Name,
                IsFinal = x.IsFinal,
                LevelId = x.LevelId.HasValue ? x.LevelId.Value : Guid.NewGuid(),
                Assignments = x.Assignments,
                Order = x.Order,
            });

            await this._eventAssignmentsService.UpdateAssignmentAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }
    }
}