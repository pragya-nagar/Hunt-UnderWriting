using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Abstracts;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.Common.Security.Attributes;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Domain;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands.Comment;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class CommentsController : Controller
    {
        private readonly IPublishMessage _publisher;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorage _fileStorage;

        public CommentsController(IPublishMessage publisher, ICurrentUserService currentUserService, IFileStorage fileStorage)
        {
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        [Route("import")]
        [HttpGet]
        [ProducesResponseType(typeof(ImportMetadataModel), 200)]
        [CheckPermission("Underwriting.ReviewComments.Write")]
        public async Task<IActionResult> GetImportUrl([FromQuery] Guid eventId, CancellationToken cancellationToken = default)
        {
            var uploadId = FileId.Generate(eventId, "comments");

            var uploadUrl = await this._fileStorage.GetUploadUrlAsync(uploadId.FileName, cancellationToken).ConfigureAwait(false);

            return this.Ok(new ImportMetadataModel { Id = uploadId.Id, UploadUrl = uploadUrl });
        }

        [Route("import/{id}")]
        [HttpPost]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.ReviewComments.Write")]
        public async Task<IActionResult> ImportFile([FromRoute]string id, CancellationToken cancellationToken = default)
        {
            var uploadId = FileId.Parse(id);

            var command = Command.Create<CommentFileProcessCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = uploadId.EventId;
            command.FileName = uploadId.FileName;

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }
    }
}