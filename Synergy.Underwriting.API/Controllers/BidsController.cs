using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Synergy.Common.Abstracts;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.Common.Security.Attributes;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Domain;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Bid;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public class BidsController : Controller
    {
        private readonly IBidService _bidService;
        private readonly IPublishMessage _publisher;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorage _fileStorage;
        private readonly IMapper _mapper;

        public BidsController(IBidService bidService,
                              IPublishMessage publisher,
                              ICurrentUserService currentUserService,
                              IFileStorage fileStorage,
                              IMapper mapper)
        {
            this._bidService = bidService ?? throw new ArgumentNullException(nameof(bidService));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [ProducesResponseType(typeof(SearchResultModel<BidModel>), 200)]
        [CheckPermission("Underwriting.EventBidList.Read")]
        public async Task<IActionResult> Get([FromQuery]SearchArgsModel<BidFilterArgs, BidSortField> args, CancellationToken cancellationToken = default)
        {
            var res = await this._bidService.GetListAsync(args, cancellationToken).ConfigureAwait(false);
            return this.Ok(res);
        }

        [Route("{id:guid}")]
        [HttpGet]
        [ProducesResponseType(typeof(BidDetailsModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Underwriting.EventBidList.Read")]
        public async Task<IActionResult> Get([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._bidService.FindAsync(id, cancellationToken).ConfigureAwait(false);
            return this.Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), 202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.EventBidList.Write")]
        public async Task<IActionResult> Post([FromBody]BidCreateArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<BidCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.AcceptedAtAction("Get", new { id = command.Id }, command.Id);
        }

        [Route("{id:guid}")]
        [HttpPut]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.EventBidList.Write")]
        public async Task<IActionResult> Put([FromRoute]Guid id, [FromBody]BidUpdateArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<BidUpdateCommand>(id, this._currentUserService.UserId);

            this._mapper.Map(args, command);

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("{id:guid}")]
        [HttpDelete]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.EventBidList.Delete")]
        public async Task<IActionResult> Delete([FromRoute]Guid id, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<BidDeleteCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            command.BidIds = new[] { id };

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("")]
        [HttpDelete]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.EventBidList.Delete")]
        public async Task<IActionResult> Delete([FromBody]BidDeleteArgs args, CancellationToken cancellationToken = default)
        {
            var command = Command.Create<BidDeleteCommand>(Guid.NewGuid(), this._currentUserService.UserId);
            command.BidIds = args.BidIds;

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("import")]
        [HttpGet]
        [ProducesResponseType(typeof(ImportMetadataModel), 200)]
        [CheckPermission("Underwriting.EventBidList.Write")]
        public async Task<IActionResult> GetImportUrl([FromQuery] Guid eventId, CancellationToken cancellationToken = default)
        {
            var uploadId = FileId.Generate(eventId, "bids");

            var uploadUrl = await this._fileStorage.GetUploadUrlAsync(uploadId.FileName, cancellationToken).ConfigureAwait(false);

            return this.Ok(new ImportMetadataModel
            {
                Id = uploadId.Id,
                UploadUrl = uploadUrl,
            });
        }

        [Route("import/{id}")]
        [HttpPost]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [CheckPermission("Underwriting.EventBidList.Write")]
        public async Task<IActionResult> ImportFile([FromRoute]string id, CancellationToken cancellationToken = default)
        {
            var uploadId = FileId.Parse(id);

            var command = Command.Create<BidFileProcessCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = uploadId.EventId;
            command.FileName = uploadId.FileName;

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return this.Accepted();
        }

        [Route("export")]
        [HttpPost]
        [ProducesResponseType(typeof(string), 202)]
        [CheckPermission("Underwriting.EventBidList.Write")]
        public async Task<IActionResult> Export([FromQuery]Guid eventId, CancellationToken cancellationToken = default)
        {
            var exportId = FileId.Generate(eventId, "bidExport", $"bids_{eventId:N}.xlsx");

            var command = Command.Create<BidExportFileCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = eventId;
            command.FileName = exportId.FileName;

            await this._publisher.PublishAsync(command, cancellationToken);

            return this.AcceptedAtAction(nameof(this.GetExportUrl), new { id = exportId.Id }, exportId.Id);
        }

        [Route("export/{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(ObjectAccessModel), 200)]
        [ProducesResponseType(404)]
        [CheckPermission("Underwriting.EventBidList.Write")]
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
    }
}