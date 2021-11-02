using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Synergy.Common.Exceptions;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Extensions.Progress;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services
{
    public class BidImportService : IMessageHandler<BidFileProcessCommand>
    {
        private readonly ILogger<BidService> _logger;
        private readonly IFileStorage _fileStorage;

        private readonly IBulkCreateBidCommand _bulkCreateBidCommand;
        private readonly IRefreshResultToBidRelationCommand _refreshResultToBidRelationCommand;
        private readonly GetBidListQuery _bidListQuery;
        private readonly IProgressPublisher _progressPublisher;

        public BidImportService(ILogger<BidService> logger,
            IFileStorage fileStorage,
            IBulkCreateBidCommand bulkCreateBidCommand,
            IRefreshResultToBidRelationCommand refreshResultToBidRelationCommand,
            GetBidListQuery bidListQuery,
            IProgressPublisher progressPublisher)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._bulkCreateBidCommand = bulkCreateBidCommand ?? throw new ArgumentNullException(nameof(bulkCreateBidCommand));
            this._refreshResultToBidRelationCommand = refreshResultToBidRelationCommand ?? throw new ArgumentNullException(nameof(refreshResultToBidRelationCommand));
            this._bidListQuery = bidListQuery ?? throw new ArgumentNullException(nameof(bidListQuery));
            this._progressPublisher = progressPublisher ?? throw new ArgumentNullException(nameof(progressPublisher));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public void Handle(BidFileProcessCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(BidFileProcessCommand message, CancellationToken cancellationToken = default)
        {
            try
            {
                var metadata = await this._fileStorage.GetMetadataAsync(message.FileName, cancellationToken).ConfigureAwait(false);
                metadata = metadata.ToDictionary(x => x.Key.ToUpperInvariant(), x => x.Value);

                if (metadata.TryGetValue("STATUS", out var status) && string.Equals(status, "processing", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotAcceptableException($"File {message.Id} already processed");
                }

                await this._fileStorage.SetMetadataAsync(message.FileName, new Dictionary<string, string> { { "STATUS", "processing" } }, cancellationToken)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                this._logger.LogInformation("Bid file {fileName} import started", message.FileName);

                var fileContent = await this._fileStorage.GetAsync(message.FileName, cancellationToken).ConfigureAwait(false);

                this._logger.LogInformation("Bid file {fileName} downloaded", message.FileName);

                if (fileContent.Length == 0)
                {
                    throw new NotAcceptableException("Content is empty");
                }

                cancellationToken.ThrowIfCancellationRequested();

                var commands = new Dictionary<string, CreateBidModel>();

                using (var memoryStream = new MemoryStream(fileContent))
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        throw new NotAcceptableException("Content is empty");
                    }

                    var rowCount = worksheet.Dimension?.Rows;
                    var colCount = worksheet.Dimension?.Columns;

                    this._logger.LogInformation("Bid file {fileName} package loaded", message.FileName);

                    if (worksheet.Dimension == null || colCount == 0 || rowCount == 0)
                    {
                        throw new NotAcceptableException("Content is empty");
                    }

                    const int expectedColCount = 3;
                    if (colCount < expectedColCount)
                    {
                        throw new NotAcceptableException($"Unexpected file format. The file should contain {expectedColCount} columns");
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    const int startRow = 2;
                    for (var rowIndex = startRow; rowIndex <= rowCount.Value; rowIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var isEmpty = true;
                        for (var i = 1; i <= colCount.Value; i++)
                        {
                            if (worksheet.Cells[rowIndex, i].Value != null)
                            {
                                isEmpty = false;
                            }
                        }

                        if (isEmpty == true)
                        {
                            continue;
                        }

                        var command = new CreateBidModel
                        {
                            Id = Guid.NewGuid(),
                            EventId = message.EventId,
                            Number = worksheet.Cells[rowIndex, 1].GetValue<string>(),
                            Entity = worksheet.Cells[rowIndex, 2].GetValue<string>(),
                            Portfolio = worksheet.Cells[rowIndex, 3].GetValue<string>(),
                        };

                        if (string.IsNullOrWhiteSpace(command.Number) == true)
                        {
                            throw new NotAcceptableException($"Bid number is empty. Row: {rowIndex}");
                        }

                        var key = command.Number.ToUpperInvariant();
                        if (commands.ContainsKey(key))
                        {
                            var ex = commands[key];
                            if (command.Entity.Equals(ex.Entity, StringComparison.OrdinalIgnoreCase) == false || command.Portfolio.Equals(ex.Portfolio, StringComparison.OrdinalIgnoreCase) == false)
                            {
                                throw new NotAcceptableException($"There is different entity for the same bid number. Row: {rowIndex}, BidNumber: {ex.Number}");
                            }

                            continue;
                        }

                        commands.Add(key, command);
                    }
                }

                if (commands.Any() == false)
                {
                    throw new NotAcceptableException("Content is empty");
                }

                this._logger.LogInformation("{count} Bids extracted from file {fileName}", commands.Count, message.FileName);

                cancellationToken.ThrowIfCancellationRequested();

                this._logger.LogInformation("Loading bids from database...");

                var existNumbers = await this._bidListQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

                this._logger.LogInformation("Loaded {count} bids from database. Start filtering {listSize} records...", existNumbers.Count, commands.Count());

                foreach (var item in existNumbers.Values)
                {
                    var key = item.Number.ToUpperInvariant();

                    if (commands.ContainsKey(key))
                    {
                        var ex = commands[key];
                        if (item.Entity.Equals(ex.Entity, StringComparison.OrdinalIgnoreCase) == false || item.Portfolio.Equals(ex.Portfolio, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            throw new NotAcceptableException($"There is different entity for the same bid number. BidNumber {item.Number}");
                        }

                        commands.Remove(key);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                existNumbers = null;

                GC.Collect();

                var bulkSize = 50000;

                var bulks = commands.Values.Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / bulkSize)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                cancellationToken.ThrowIfCancellationRequested();

                var progress = 30.0;
                var progressPerBatch = (90 - progress) / bulks.Count;
                await this._progressPublisher.PostProgressAsync((int)progress, cancellationToken).ConfigureAwait(false);

                using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(20),
                },
                TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var (bulk, num) in bulks.Select((x, i) => (x, i + 1)))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        this._logger.LogInformation("Starting bid bulk create dispatch for bulk {num} of {cnt}. Bulk size {size}", num, bulks.Count(), bulk.Count);

                        await this._bulkCreateBidCommand.DispatchAsync(bulk, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                        this._logger.LogInformation("Bid bulk {num} create finished", num);

                        progress += progressPerBatch;
                        await this._progressPublisher.PostProgressAsync((int)progress, cancellationToken).ConfigureAwait(false);
                    }

                    this._logger.LogInformation("Starting RefreshResultToBidRelationCommand");

                    await this._refreshResultToBidRelationCommand.DispatchAsync(new RefreshResultToBidRelationModel { Id = message.EventId }, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                    this._logger.LogInformation("RefreshResultToBidRelationCommand finished");

                    scope.Complete();
                }

                await this.SetProcessingSuccessAsync(message.FileName).ConfigureAwait(false);
            }
            catch (ApplicationException ex)
            {
                await this.SetProcessingErrorAsync(message.FileName, ex.Message).ConfigureAwait(false);
                throw;
            }
        }

        private async Task SetProcessingStatusAsync(string fileName, string status, string error)
        {
            var metadata = new Dictionary<string, string>
            {
                { "STATUS", status },
                { "ERROR", error },
            };

            await this._fileStorage.SetMetadataAsync(fileName, metadata).ConfigureAwait(false);

            this._logger.LogInformation("Bid file {fileName} status changed to {status} with error message {error}", fileName, status, error);
        }

        private Task SetProcessingSuccessAsync(string fileName)
        {
            return this.SetProcessingStatusAsync(fileName, "success", null);
        }

        private Task SetProcessingErrorAsync(string fileName, string error)
        {
            return this.SetProcessingStatusAsync(fileName, "error", error);
        }
    }
}