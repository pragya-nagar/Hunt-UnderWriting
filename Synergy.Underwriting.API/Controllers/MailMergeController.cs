using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Abstracts;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.Common.Security.Attributes;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Domain;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class MailMergeController : Controller
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IPublishMessage _publisher;
        private readonly IFileStorage _fileStorage;

        public MailMergeController(ICurrentUserService currentUserService, IPublishMessage publisher, IFileStorage fileStorage)
        {
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        [Route("delinquencies")]
        [HttpGet]
        [ProducesResponseType(typeof(ImportMetadataModel), 200)]
        [ProducesDefaultResponseType]
        [CheckPermission("Underwriting.MailMerge.Read")]
        public async Task<IActionResult> GetImportUrl([FromQuery] Guid eventId, CancellationToken cancellationToken = default)
        {
            var uploadId = FileId.Generate(eventId, "delinquencies");

            var uploadUrl = await this._fileStorage.GetUploadUrlAsync(uploadId.FileName, cancellationToken).ConfigureAwait(false);

            return this.Ok(new ImportMetadataModel
            {
                Id = uploadId.Id,
                UploadUrl = uploadUrl,
            });
        }

        [Route("{fileId}")]
        [HttpGet]
        [ProducesResponseType(typeof(ImportMetadataModel), 200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [CheckPermission("Underwriting.MailMerge.Read")]
        public async Task<IActionResult> GetUploadUrl(string fileId, CancellationToken cancellationToken = default)
        {
            var file = FileId.Parse(fileId);

            var access = await this._fileStorage.GetAccessAsync(file.FileName, cancellationToken).ConfigureAwait(false);

            var item = access.FirstOrDefault();
            if (item == null)
            {
                return this.NotFound();
            }

            return this.Ok(item);
        }

        [Route("{templateId:guid}")]
        [HttpPost]
        [ProducesResponseType(typeof(Guid), 202)]
        [ProducesResponseType(400)]
        [ProducesDefaultResponseType]
        [CheckPermission("Underwriting.MailMerge.Read")]
        public async Task<ActionResult> Merge(Guid templateId, string delinquencyFileId, CancellationToken cancellationToken = default)
        {
            var delinquencyFile = FileId.Parse(delinquencyFileId);
            var resultFile = FileId.Generate(delinquencyFile.EventId, "mergeresult");

            var command = Command.Create<MailMergeCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            command.EventId = delinquencyFile.EventId;
            command.DeliquencyPath = delinquencyFile.FileName;
            command.TemplateId = templateId;
            command.ResultPath = resultFile.FileName;

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.AcceptedAtAction(nameof(this.GetUploadUrl), new { fileId = resultFile.Id }, resultFile.Id);
        }
    }
}
