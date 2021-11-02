using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Abstracts;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.Common.Security.Attributes;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Messages.Events;
using Synergy.Underwriting.Domain;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Event;
using Synergy.Underwriting.Models.EventAssignment;
using Synergy.Underwriting.Models.Rule;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;

        private readonly IPublishMessage _publisher;

        private readonly IFileStorage _fileStorage;

        private readonly ICurrentUserService _currentUserService;

        private readonly IMapper _mapper;

        public EventsController(IEventService eventService,
                                IPublishMessage publisher,
                                IFileStorage fileStorage,
                                ICurrentUserService currentUserService,
                                IMapper mapper)
        {
            this._eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this._publisher = publisher;
            this._fileStorage = fileStorage;
            this._currentUserService = currentUserService;
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<EventModel>), 200)]
        [CheckPermission("Admin.Event.Read")]
        public async Task<IActionResult> Get([FromQuery]SearchArgsModel<EventFilterArgs, EventSortField> args, CancellationToken cancellationToken = default)
        {
            var res = await this._eventService.GetListAsync(args, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }

        [Route("{id:guid}")]
        [HttpGet]
        [ProducesResponseType(typeof(EventDetailsModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Admin.Event.Read")]
        public async Task<IActionResult> Get([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._eventService.FindAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(item);
        }

        [Route("{id:guid}/calculated")]
        [HttpGet]
        [ProducesResponseType(typeof(EventCalculatedFieldsModel), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [CheckPermission("Admin.Event.Read")]
        public async Task<IActionResult> GetCalculated([FromRoute]Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var item = await this._eventService.FindEventCalculatedData(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), 202)]
        [ProducesResponseType(400)]
        [CheckPermission("Admin.Event.Write")]
        public async Task<IActionResult> Post([FromBody]EventCreateArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<EventCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.AcceptedAtAction("Get", new { id = command.Id }, command.Id);
        }

        [Route("{id:guid}")]
        [HttpPut]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Admin.Event.Write")]
        public async Task<IActionResult> Put([FromRoute]Guid id, [FromBody]EventUpdateArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<EventUpdateCommand>(id, this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("{id:guid}/attachments")]
        [HttpGet]
        [ProducesResponseType(typeof(Guid), 200)]
        [CheckPermission("Admin.Attachments.Write")]
        public async Task<IActionResult> GetUploadAttachmentUrl(Guid id, string fileName, CancellationToken cancellationToken = default)
        {
            var uploadId = AttachmentId.Generate("event_attachment", id, fileName);

            var uploadUrl = await this._fileStorage.GetUploadUrlAsync(uploadId.FileName, cancellationToken).ConfigureAwait(false);

            return this.Ok(new ImportMetadataModel
            {
                Id = uploadId.Id,
                UploadUrl = uploadUrl,
            });
        }

        [Route("{id:guid}/attachments")]
        [HttpPost]
        [ProducesResponseType(typeof(Guid), 202)]
        [ProducesResponseType(400)]
        [CheckPermission("Admin.Event.Write")]
        public async Task<IActionResult> CreateAttachment([FromRoute]Guid id, string fileId, CancellationToken cancellationToken = default)
        {
            var attachment = AttachmentId.Parse(fileId);

            if (attachment.EntityId != id)
            {
                return this.BadRequest("Invalid file id.");
            }

            if (attachment.EntityType != "event_attachment")
            {
                return this.BadRequest("Invalid file id.");
            }

            var command = Command.Create<AttachmentCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            command.EventId = id;
            command.Path = attachment.FileName;
            command.FileName = attachment.FriendlyName;

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.AcceptedAtAction("DownloadAttachment", new { id = command.Id }, command.Id);
        }

        [Route("attachments/{id:guid}")]
        [HttpGet]
        [ProducesResponseType(200)]
        [CheckPermission("Admin.Event.Read")]
        public async Task<IActionResult> DownloadAttachment([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._eventService.FindAttachmentAsync(id, cancellationToken).ConfigureAwait(false);

            return this.Ok(item);
        }

        [Route("attachments/{id:guid}")]
        [HttpDelete]
        [ProducesResponseType(202)]
        [CheckPermission("Admin.Event.Write")]
        public async Task<IActionResult> DeleteAttachment([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            await this._eventService.DeleteAttachmentAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Accepted();
        }

        [Route("{id:guid}/levels")]
        [HttpGet]
        [ProducesResponseType(typeof(EventAssignmentResult), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Admin.EventAssignment.Read")]
        public async Task<IActionResult> GetLevels([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._eventService.GetLevelListAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(item);
        }

        [Route("{id:guid}/levels")]
        [HttpPost]
        [CheckPermission("Admin.EventAssignment.Write")]
        [ProducesResponseType(typeof(Guid), 202)]

        public async Task<IActionResult> PostLevel([FromRoute]Guid id, [FromBody]IEnumerable<AssignmentLevelCreateArgs> args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<AssignmentLevelCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = id;
            command.Assigments = args.Select(x => new AssigmentCreateModel
            {
                Name = x.Name,
                IsFinal = x.IsFinal,
                LevelId = Guid.NewGuid(),
                Assignments = x.Assignments,
                Order = x.Order,
            });

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("{id:guid}/levels")]
        [HttpPut]
        [ProducesResponseType(202)]
        [CheckPermission("Admin.EventAssignment.Write")]
        public async Task<IActionResult> PutLevel([FromRoute]Guid id, [FromBody]IEnumerable<AssignmentLevelUpdateArgs> args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<AssignmentLevelUpdateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = id;

            command.Assigments = args.Select(x => new AssigmentUpdateModel
            {
                LevelId = x.LevelId,
                Assignments = x.Assignments,
            });

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [HttpGet]
        [Route("{id:guid}/rules")]
        [ProducesResponseType(typeof(SearchResultModel<RuleModel>), 200)]
        [CheckPermission("Admin.EventDataCut.Read")]
        public async Task<IActionResult> GetRules([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var items = await this._eventService.GetRuleListAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(items);
        }

        [CheckPermission("Admin.EventDataCut.Write")]
        [Route("rules")]
        [HttpPost]
        [ProducesResponseType(202)]
        public async Task<IActionResult> PostRules([FromBody]CreateRuleArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<RuleCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            this._mapper.Map(args, command);
            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted(command.Id);
        }

        [Route("{id:guid}/rules")]
        [HttpPut]
        [ProducesResponseType(202)]
        [CheckPermission("Admin.EventDataCut.Write")]
        public async Task<IActionResult> PutRules([FromRoute]Guid id, [FromBody]IEnumerable<Guid> args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<ApplyRulesCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            command.EventId = id;
            command.RuleIds = args;
            await this._publisher.PublishAsync(command, cancellationToken);

            return this.Accepted();
        }

        [Route("{id:guid}/dump/fields")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [CheckPermission("Admin.EventDataDump.Read")]
        public async Task<IActionResult> GetDumpFields([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var res = await this._eventService.GetDumpFieldsAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }

        [Route("{id:guid}/dump")]
        [HttpPost]
        [ProducesResponseType(202)]
        [CheckPermission("Admin.EventDataDump.Read")]
        public async Task<IActionResult> CreateDump(Guid id, [FromBody]IEnumerable<EventDumpField> fields, CancellationToken cancellationToken = default)
        {
            var exportId = FileId.Generate(id, "eventExport", $"dump_{id:N}.xlsx");

            var command = Command.Create<EventDumpFileCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = id;
            command.FileName = exportId.FileName;
            command.Fields = fields;

            await this._publisher.PublishAsync(command, cancellationToken);

            return this.AcceptedAtAction(nameof(this.GetExportUrl), new { id = exportId.Id }, exportId.Id);
        }

        [Route("{id}/dump")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectAccessModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Admin.EventDataDump.Read")]
        public async Task<IActionResult> GetExportUrl([FromRoute]string id, CancellationToken cancellationToken = default)
        {
            var exportId = FileId.Parse(id);

            var list = await this._fileStorage.GetAccessAsync(exportId.FileName, cancellationToken).ConfigureAwait(false);

            var item = list.FirstOrDefault();
            if (item == null)
            {
                return this.NotFound();
            }

            return this.Ok(item);
        }

        [HttpPatch]
        [Route("{id:guid}/SetLockStatus")]
        [ProducesResponseType(202)]
        [CheckPermission("Admin.Event.Lock")]
        public async Task<IActionResult> PatchSetLockStatus([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            await this._eventService.SetLockStatusAsync(id, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("{id:guid}/etlcomplete")]
        [HttpPut]
        [ProducesResponseType(202)]
        public async Task<IActionResult> PutETLComplete(CancellationToken cancellationToken = default)
        {
            ETLProcessingFinishedEvent etlEvent = Event.Create<ETLProcessingFinishedEvent>(Guid.NewGuid(), this._currentUserService.UserId);
            etlEvent.EndTime = DateTime.UtcNow;
            await this._publisher.PublishAsync(etlEvent, cancellationToken);

            return this.Accepted();
        }

        [Route("{id:guid}/rulesdump")]
        [HttpPost]
        [ProducesResponseType(202)]
        [CheckPermission("Admin.EventDataCut.Read")]
        public async Task<IActionResult> CreateRulesDump([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var exportId = await this._eventService.CreateRulesDumpAsync(id, cancellationToken).ConfigureAwait(false);

            return this.AcceptedAtAction(nameof(this.GetRulesExportUrl), new { id = exportId.Id }, exportId.Id);
        }

        [Route("{id}/rulesdump")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectAccessModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Admin.EventDataCut.Read")]
        public async Task<IActionResult> GetRulesExportUrl([FromRoute]string id, CancellationToken cancellationToken = default)
        {
            var exportId = FileId.Parse(id);

            var list = await this._fileStorage.GetAccessAsync(exportId.FileName, cancellationToken).ConfigureAwait(false);

            var item = list.FirstOrDefault();
            if (item == null)
            {
                return this.NotFound();
            }

            return this.Ok(item);
        }

        [Route("reviewReport")]
        [HttpPost]
        [ProducesResponseType(202)]
        [CheckPermission("Admin.Event.Read")]
        public async Task<IActionResult> CreateReviewDump([FromBody]ReviewReportArgs reviewReportArgs, CancellationToken cancellationToken = default)
        {
            string date = DateTime.UtcNow.ToString("M_d_yyyy", CultureInfo.InvariantCulture);
            FileId exportId;
            if (reviewReportArgs.IsPerUserReport)
            {
                exportId = FileId.Generate(Guid.NewGuid(), "reviewReportExport", $"Reviewers_report_{date:N}.xlsx");
            }
            else
            {
                exportId = FileId.Generate(Guid.NewGuid(), "reviewReportExport", $"review_report_{date:N}.xlsx");
            }

            var command = Command.Create<ReviewDumpCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.FileName = exportId.FileName;
            command.StateId = reviewReportArgs.StateId;
            command.SaleDateFrom = reviewReportArgs.SaleDateFrom;
            command.SaleDateTo = reviewReportArgs.SaleDateTo;
            command.IsPerUserReport = reviewReportArgs.IsPerUserReport;
            command.IsEventLocked = reviewReportArgs.IsEventLocked;

            await this._publisher.PublishAsync(command, cancellationToken);

            return this.AcceptedAtAction(nameof(this.GetExportUrl), new { id = exportId.Id }, exportId.Id);
        }

        [Route("{id}/reviewReport")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectAccessModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Admin.Event.Read")]
        public async Task<IActionResult> GetReviewReportExportUrl([FromRoute]string id, CancellationToken cancellationToken = default)
        {
            var exportId = FileId.Parse(id);

            var list = await this._fileStorage.GetAccessAsync(exportId.FileName, cancellationToken).ConfigureAwait(false);

            var item = list.FirstOrDefault();
            if (item == null)
            {
                return this.NotFound();
            }

            return this.Ok(item);
        }
    }
}