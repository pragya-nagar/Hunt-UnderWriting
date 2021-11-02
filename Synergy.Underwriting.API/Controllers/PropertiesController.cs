using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Abstracts;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.Common.Security.Attributes;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Domain;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Comment;
using Synergy.Underwriting.Models.Property;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class PropertiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPublishMessage _publisher;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPropertyService _propertyService;
        private readonly IFileStorage _fileStorage;

        public PropertiesController(
            IPublishMessage publisher,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IPropertyService propertyService,
            IFileStorage fileStorage)
        {
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<PropertyModel>), 200)]
        [CheckPermission("Underwriting.ReviewPage.Read")]
        public async Task<IActionResult> Get([FromQuery]SearchArgsModel<PropertyFilterArgs, PropertySortField> args, CancellationToken cancellationToken = default)
        {
            var res = await this._propertyService.GetListAsync(args, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }

        [Route("{id:guid}")]
        [HttpPut]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.ReviewPage.Write")]
        public async Task<IActionResult> Put([FromRoute]Guid id, [FromBody]PropertyUpdateArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<PropertyUpdateCommand>(id, this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [HttpGet]
        [Route("levels")]
        [ProducesResponseType(typeof(SearchResultModel<DecisionLevelModel>), 200)]
        [CheckPermission("Underwriting.ReviewPage.Read")]
        public async Task<IActionResult> GetLevels([FromQuery]SearchArgsModel<DecisionLevelFilterArgs, DecisionLevelSortField> args, CancellationToken cancellationToken = default)
        {
            var res = await this._propertyService.GetLevelListAsync(args, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }

        [Route("{id:guid}/decision")]
        [HttpPut]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.ReviewPage.Write")]
        public async Task<IActionResult> MakeDecision([FromRoute]Guid id, [FromBody]MakeDecisionArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<MakeDecisionCommand>(id, this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("{id:guid}/photos")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ObjectAccessModel>), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Admin.ReviewPage.Read")]
        public async Task<IActionResult> GetPhotos([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var res = await this._propertyService.GetPhotoUrlListAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }

        [Route("{id:guid}/attachments")]
        [HttpGet]
        [ProducesResponseType(typeof(Guid), 200)]
        [CheckPermission("Underwriting.Attachments.Write")]
        public async Task<IActionResult> GetUploadAttachmentUrl(Guid id, string fileName, CancellationToken cancellationToken = default)
        {
            var uploadId = AttachmentId.Generate("property_attachment", id, fileName);

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
        [CheckPermission("Underwriting.Attachments.Write")]
        public async Task<IActionResult> CreateAttachment([FromRoute]Guid id, string fileId, CancellationToken cancellationToken = default)
        {
            var attachment = AttachmentId.Parse(fileId);

            if (attachment.EntityId != id)
            {
                return this.BadRequest("Invalid file id.");
            }

            if (attachment.EntityType != "property_attachment")
            {
                return this.BadRequest("Invalid file id.");
            }

            var command = Command.Create<PropertyAttachmentCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            command.DelinquencyId = id;
            command.Path = attachment.FileName;
            command.FileName = attachment.FriendlyName;

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.AcceptedAtAction("DownloadAttachment", new { id = command.Id }, command.Id);
        }

        [Route("attachments/{id:guid}")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [CheckPermission("Underwriting.Attachments.Read")]
        public async Task<IActionResult> DownloadAttachment([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._propertyService.FindAttachmentAsync(id, cancellationToken).ConfigureAwait(false);

            return this.Ok(item);
        }

        [Route("attachments/{id:guid}")]
        [HttpDelete]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [CheckPermission("Underwriting.Attachments.Delete")]
        public async Task<IActionResult> DeleteAttachment([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            await this._propertyService.DeleteAttachmentAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Accepted();
        }

        [HttpPost]
        [Route("{id:guid}/comments")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [CheckPermission("Underwriting.ReviewComments.Write")]
        public async Task<IActionResult> Post([FromRoute]Guid id, [FromBody]string comment, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return this.BadRequest(new { comment = "Comment is required" });
            }

            var commentId = Guid.NewGuid();

            var command = Command.Create<DelinquencyCommentCreateCommand>(commentId, this._currentUserService.UserId);

            command.DelinquencyId = id;
            command.Comment = comment;

            await this._publisher.PublishAsync(command, cancellationToken);

            return this.AcceptedAtAction(nameof(this.GetComments), new { id }, commentId);
        }

        [HttpPut]
        [Route("{id:guid}/comments/{commentId:guid}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [CheckPermission("Underwriting.ReviewComments.Write")]
        public async Task<IActionResult> Put([FromRoute]Guid id, [FromRoute]Guid commentId, [FromBody]string comment, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return this.BadRequest(new { comment = "Comment is required" });
            }

            var commentEntity = await this._propertyService.GetCommentAsync(id, commentId, cancellationToken).ConfigureAwait(false);

            if (commentEntity == null)
            {
                return this.NotFound(new { commentId = "Comment is not found" });
            }

            if (commentEntity.Author.Id != this._currentUserService.UserId)
            {
                return this.Forbid();
            }

            var command = Command.Create<DelinquencyCommentUpdateCommand>(commentId, this._currentUserService.UserId);

            command.Comment = comment;

            await this._publisher.PublishAsync(command, cancellationToken);

            return this.AcceptedAtAction(nameof(this.GetComments), new { id }, commentId);
        }

        [HttpDelete]
        [Route("{id:guid}/comments/{commentId:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [CheckPermission("Underwriting.ReviewComments.Write")]
        public async Task<IActionResult> Delete([FromRoute]Guid id, [FromRoute]Guid commentId, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<DelinquencyCommentDeleteCommand>(commentId, this._currentUserService.UserId);

            var commentEntity = await this._propertyService.GetCommentAsync(id, commentId, cancellationToken).ConfigureAwait(false);

            if (commentEntity == null)
            {
                return this.NotFound(new { commentId = "Comment is not found" });
            }

            if (commentEntity.Author.Id != this._currentUserService.UserId)
            {
                return this.Forbid();
            }

            await this._publisher.PublishAsync(command, cancellationToken);

            return this.Accepted();
        }

        [HttpGet]
        [Route("{id:guid}/comments")]
        [ProducesResponseType(400)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [CheckPermission("Underwriting.ReviewComments.Read")]
        public async Task<IActionResult> GetComments([FromRoute] Guid id, [FromQuery] SearchArgsModel args, CancellationToken cancellationToken = default)
        {
            var searchArgs = new SearchArgsModel<Guid, CommentSortField>() { Filter = id, Limit = args.Limit, Offset = args.Offset };

            var comments = await this._propertyService.GetCommentsListAsync(searchArgs, cancellationToken);

            return this.Ok(comments);
        }

        [HttpGet]
        [Route("{id:guid}/snapshot/{date:datetime}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [CheckPermission("Underwriting.ReviewPage.Read")]
        public async Task<IActionResult> GetSnapshot([FromRoute]Guid id, DateTime? date, CancellationToken cancellationToken = default)
        {
            var items = await this._propertyService.GetSnapshotAsync(id, date, cancellationToken).ConfigureAwait(false);
            return this.Ok(items);
        }

        [HttpGet]
        [Route("{id:guid}/decision/history")]
        [ProducesResponseType(400)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [CheckPermission("Underwriting.ReviewPage.Read")]
        public async Task<IActionResult> GetDecisionHistory([FromRoute]Guid id, [FromQuery]SearchArgsModel args, CancellationToken cancellationToken = default)
        {
            var items = await this._propertyService.GetDecisionHistoryAsync(new SearchArgsModel<Guid, int> { Filter = id, SortField = 0, Limit = args.Limit, Offset = args.Offset }, cancellationToken);
            return this.Ok(items);
        }

        [Route("{id:guid}/uploadimage")]
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        [CheckPermission("Admin.ReviewPage.Read")]
        public async Task<IActionResult> UploadImageUrl([FromQuery] Guid propertyId, CancellationToken cancellationToken = default)
        {
            var uploadId = PropertyFileId.Generate(propertyId, "property");

            var uploadUrl = await this._fileStorage.GetUploadUrlAsync(uploadId.FileName, cancellationToken).ConfigureAwait(false);

            return this.Ok(new ImportMetadataModel { Id = uploadId.Id, UploadUrl = uploadUrl });
        }
    }
}
