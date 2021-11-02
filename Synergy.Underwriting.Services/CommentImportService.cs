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
using Synergy.Underwriting.DAL.Commands.Models.Results;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands.Comment;

namespace Synergy.Underwriting.Services
{
    public class CommentImportService : IMessageHandler<CommentFileProcessCommand>
    {
        private readonly GetDelinquencyListQuery _delinquencyListQuery;

        private readonly IFileStorage _fileStorage;
        private readonly ILogger<CommentImportService> _logger;
        private readonly IProgressPublisher _progressPublisher;
        private readonly IBulkCreateCommentCommand _bulkCreateCommentCommand;

        public CommentImportService(
            GetDelinquencyListQuery delinquencyListQuery,
            IFileStorage fileStorage,
            ILogger<CommentImportService> logger,
            IProgressPublisher progressPublisher,
            IBulkCreateCommentCommand bulkCreateCommentCommand)
        {
            this._delinquencyListQuery = delinquencyListQuery ?? throw new ArgumentNullException(nameof(delinquencyListQuery));

            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._progressPublisher = progressPublisher ?? throw new ArgumentNullException(nameof(progressPublisher));
            this._bulkCreateCommentCommand = bulkCreateCommentCommand ?? throw new ArgumentNullException(nameof(bulkCreateCommentCommand));
        }

        public void Handle(CommentFileProcessCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(CommentFileProcessCommand message, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.InternalHandleAsync(message, cancellationToken).ConfigureAwait(false);

                await this.SetProcessingSuccessAsync(message.FileName).ConfigureAwait(false);
            }
            catch (ApplicationException ex)
            {
                this._logger.LogError(ex, "Unable to process comments file.");

                await this.SetProcessingErrorAsync(message.FileName, ex.Message).ConfigureAwait(false);

                throw;
            }
        }

        private static CommentModel MapRow(ExcelWorksheet worksheet, int rowIndex)
        {
            return new CommentModel
            {
                AdvertisementNumber = worksheet.Cells[rowIndex, 1].GetValue<string>(),
                ParcelId = worksheet.Cells[rowIndex, 2].GetValue<string>(),
                Comment = worksheet.Cells[rowIndex, 3].GetValue<string>(),
            };
        }

        private static string ValidateRecord(CommentModel args)
        {
            if (string.IsNullOrWhiteSpace(args.Comment))
            {
                return $"Comment is required";
            }

            if (string.IsNullOrWhiteSpace(args.ParcelId) && string.IsNullOrWhiteSpace(args.AdvertisementNumber))
            {
                return $"Parcel Id or Advertisement Number is required";
            }

            return null;
        }

        private async Task InternalHandleAsync(CommentFileProcessCommand message, CancellationToken cancellationToken)
        {
            var metadata = await this._fileStorage.GetMetadataAsync(message.FileName, cancellationToken).ConfigureAwait(false);

            metadata = metadata.ToDictionary(x => x.Key.ToUpperInvariant(), x => x.Value);

            if (metadata.TryGetValue("STATUS", out var status) && string.Equals(status, "processing", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotAcceptableException($"File {message.Id} already processed");
            }

            await this._fileStorage.SetMetadataAsync(message.FileName, new Dictionary<string, string> { { "STATUS", "processing" } }, cancellationToken)
                                   .ConfigureAwait(false);

            this._logger.LogInformation("Starting comments import");

            var content = await this._fileStorage.GetAsync(message.FileName, cancellationToken).ConfigureAwait(false);

            if (content.Length == 0)
            {
                throw new NotAcceptableException("Content is empty");
            }

            this._logger.LogInformation("Import file content uploaded. Uploaded {length} bytes.", content.Length);

            var list = new List<CommentModel>();

            using (var memoryStream = new MemoryStream(content))
            using (var package = new ExcelPackage(memoryStream))
            {
                this._logger.LogInformation("Excel package created.");

                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    throw new NotAcceptableException("Content is empty");
                }

                var rowCount = worksheet.Dimension?.Rows;
                var colCount = worksheet.Dimension?.Columns;

                if (rowCount == null)
                {
                    throw new NotAcceptableException("Content is empty");
                }

                const int expectedColCount = 3;

                if (colCount < expectedColCount)
                {
                    throw new NotAcceptableException($"Unexpected file format. The file should contain {expectedColCount} columns");
                }

                this._logger.LogInformation("Start reading excel data {rowsCount} rows.", rowCount.Value);

                const int startRow = 2;
                for (var rowIndex = startRow; rowIndex <= rowCount.Value; rowIndex++)
                {
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

                    try
                    {
                        var item = MapRow(worksheet, rowIndex);

                        var error = ValidateRecord(item);

                        if (string.IsNullOrWhiteSpace(error) == false)
                        {
                            throw new NotAcceptableException(error);
                        }

                        list.Add(item);
                    }
                    catch (FormatException)
                    {
                        throw new NotAcceptableException($"Row {rowIndex} has incorrect format");
                    }
                    catch (InvalidCastException)
                    {
                        throw new NotAcceptableException($"Row {rowIndex} has incorrect format");
                    }
                }

                this._logger.LogInformation("Reading excel data rows finished.");
            }

            this._logger.LogInformation("Start loading delinquencies from database");

            var delinquencyList = await this._delinquencyListQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            if (delinquencyList.Any() == false)
            {
                throw new NotAcceptableException($"Event '{message.EventId}' does not contain delinquencies");
            }

            this._logger.LogInformation("{recordsCount} delinquencies loaded  from database", delinquencyList.Count());

            var delinquencyMap = list
                .GroupJoin(delinquencyList,
                    x => x.AdvertisementNumber,
                    x => x.AdvertisementNumber,
                    (c, d) => new
                    {
                        Comment = c,
                        DelinquencyList = string.IsNullOrWhiteSpace(c.AdvertisementNumber) == false
                                            ? d.Where(x => x.AdvertisementNumber == c.AdvertisementNumber)
                                            : null,
                    })
                .GroupJoin(delinquencyList,
                    x => x.Comment.ParcelId,
                    x => x.ParcelId,
                    (r, d) => new
                    {
                        Comment = r.Comment,
                        DelinquencyList = d.Union(r.DelinquencyList ?? Enumerable.Empty<DelinquencyModel>()),
                    })
                .ToList();

            if (delinquencyMap.Any(x => x.DelinquencyList?.Any() != true))
            {
                throw new NotAcceptableException("Please double-check the attachment, provided Delinquencies are from different batch.");
            }

            this._logger.LogInformation("Delinquency/Results map created");

            list = null;

            const int maxBatchSize = 50000;

            var batches = delinquencyMap
                    .SelectMany(x => x.DelinquencyList, (c, d) => new { c.Comment, DelinquencyId = d.Id })
                    .Distinct()
                    .Select((map, i) => (map, i))
                    .GroupBy(x => x.i / maxBatchSize, e => new CreateCommentModel
                    {
                        Id = Guid.NewGuid(),
                        Comment = e.map.Comment.Comment,
                        DelinquencyId = e.map.DelinquencyId,
                    })
                    .ToList();

            var progress = 30.0;
            var progressPerBatch = (90 - progress) / batches.Count;

            await this._progressPublisher.PostProgressAsync((int)progress, cancellationToken).ConfigureAwait(false);

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(20),
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var totalBatches = batches.Count;

                foreach (var (batch, index) in batches.Select((x, i) => (x, i)))
                {
                    this._logger.LogInformation("Start processing batch {index} of {batchesCount}.", index + 1, totalBatches);

                    var listSize = batch.Count();

                    this._logger.LogInformation("Start BulkCreateResultCommand for {cnt} items.", batch.Count());

                    await this._bulkCreateCommentCommand
                        .DispatchAsync(batch, message.CreatedBy, cancellationToken)
                        .ConfigureAwait(false);

                    this._logger.LogInformation("BulkCreateResultCommand for {cnt} items finished in batch {index} of {batchesCount}.", listSize, index + 1, totalBatches);

                    progress += progressPerBatch;

                    await this._progressPublisher.PostProgressAsync((int)progress, cancellationToken).ConfigureAwait(false);
                }

                scope.Complete();

                this._logger.LogInformation("Transaction committed.");
            }

            await this.SetProcessingSuccessAsync(message.FileName).ConfigureAwait(false);
        }

        private async Task SetProcessingStatusAsync(string fileName, string status, string error)
        {
            var metadata = new Dictionary<string, string>
            {
                { "STATUS", status },
                { "ERROR", error },
            };

            await this._fileStorage.SetMetadataAsync(fileName, metadata).ConfigureAwait(false);

            this._logger.LogInformation("Result file {fileName} status changed to {status} with error message {error}", fileName, status, error);
        }

        private Task SetProcessingSuccessAsync(string fileName)
        {
            return this.SetProcessingStatusAsync(fileName, "SUCCESS", null);
        }

        private Task SetProcessingErrorAsync(string fileName, string error)
        {
            return this.SetProcessingStatusAsync(fileName, "ERROR", error);
        }

        private class CommentModel
        {
            public string ParcelId { get; set; }

            public string Comment { get; set; }

            public string AdvertisementNumber { get; set; }
        }
    }
}
