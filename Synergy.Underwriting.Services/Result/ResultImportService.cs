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
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services
{
    public class ResultImportService : IMessageHandler<ResultFileProcessCommand>
    {
        private readonly GetDelinquencyListQuery _delinquencyListQuery;

        private readonly IBulkCreateResultCommand _bulkCreateResultCommand;
        private readonly IBulkUpdateResultCommand _bulkUpdateResultCommand;
        private readonly IRefreshResultToBidRelationCommand _refreshResultToBidRelationCommand;
        private readonly IFileStorage _fileStorage;
        private readonly ILogger<ResultService> _logger;
        private readonly IProgressPublisher _progressPublisher;

        public ResultImportService(
            GetDelinquencyListQuery delinquencyListQuery,
            IBulkCreateResultCommand bulkCreateResultCommand,
            IBulkUpdateResultCommand bulkUpdateResultCommand,
            IRefreshResultToBidRelationCommand refreshResultToBidRelationCommand,
            IFileStorage fileStorage,
            ILogger<ResultService> logger,
            IProgressPublisher progressPublisher)
        {
            this._delinquencyListQuery = delinquencyListQuery ?? throw new ArgumentNullException(nameof(delinquencyListQuery));

            this._bulkCreateResultCommand = bulkCreateResultCommand ?? throw new ArgumentNullException(nameof(bulkCreateResultCommand));
            this._bulkUpdateResultCommand = bulkUpdateResultCommand ?? throw new ArgumentNullException(nameof(bulkUpdateResultCommand));
            this._refreshResultToBidRelationCommand = refreshResultToBidRelationCommand ?? throw new ArgumentNullException(nameof(refreshResultToBidRelationCommand));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._progressPublisher = progressPublisher ?? throw new ArgumentNullException(nameof(progressPublisher));
        }

        public void Handle(ResultFileProcessCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(ResultFileProcessCommand message, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.InternalHandleAsync(message, cancellationToken).ConfigureAwait(false);

                await this.SetProcessingSuccessAsync(message.FileName).ConfigureAwait(false);
            }
            catch (ApplicationException ex)
            {
                this._logger.LogError(ex, "Unable to process result file.");

                await this.SetProcessingErrorAsync(message.FileName, ex.Message).ConfigureAwait(false);

                throw;
            }
        }

        private static ResultCreateArgs MapRow(ExcelWorksheet worksheet, int rowIndex)
        {
            var item = new ResultCreateArgs
            {
                AdvertisementNumber = worksheet.Cells[rowIndex, 1].GetValue<string>(),
                ParcelId = worksheet.Cells[rowIndex, 2].GetValue<string>(),
                BidNumber = worksheet.Cells[rowIndex, 3].GetValue<string>(),
                CertNo = worksheet.Cells[rowIndex, 4].GetValue<string>(),
                TaxYear = worksheet.Cells[rowIndex, 5].GetValue<int>(),
                TaxAmount = worksheet.Cells[rowIndex, 6].GetValue<decimal>(),
                Overbid = worksheet.Cells[rowIndex, 7].GetValue<decimal?>(),
                Premium = worksheet.Cells[rowIndex, 8].GetValue<decimal?>(),
                InterestRate = worksheet.Cells[rowIndex, 9].GetValue<decimal>(),
                PenaltyRate = worksheet.Cells[rowIndex, 10].GetValue<decimal?>(),
                RecoverableFees = worksheet.Cells[rowIndex, 11].GetValue<decimal?>(),
                NonRecoverableFees = worksheet.Cells[rowIndex, 12].GetValue<decimal?>(),
            };
            return item;
        }

        private static string ValidateRecord(ResultCreateArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.BidNumber))
            {
                return $"Bid Number is required";
            }

            if (string.IsNullOrWhiteSpace(args.ParcelId)
                && string.IsNullOrWhiteSpace(args.AdvertisementNumber))
            {
                return $"Parcel Id or Advertisement Number is required";
            }

            if (args.TaxAmount < 0)
            {
                return "Tax Amount can not be negative";
            }

            if (args.Overbid < 0)
            {
                return "Overbid can not be negative";
            }

            if (args.Premium < 0)
            {
                return "Premium can not be negative";
            }

            if (args.InterestRate < 0 || args.InterestRate > 50)
            {
                return "Interest Rate shoud be in range 0..50";
            }

            if (args.PenaltyRate < 0 || args.PenaltyRate > 50)
            {
                return "Penalty Rate shoud be in range 0..50";
            }

            if (args.NonRecoverableFees < 0)
            {
                return "Non Recoverable Fees can not be negative";
            }

            if (args.RecoverableFees < 0)
            {
                return "Recoverable Fees can not be negative";
            }

            return null;
        }

        private async Task InternalHandleAsync(ResultFileProcessCommand message, CancellationToken cancellationToken)
        {
            var metadata = await this._fileStorage.GetMetadataAsync(message.FileName, cancellationToken).ConfigureAwait(false);
            metadata = metadata.ToDictionary(x => x.Key.ToUpperInvariant(), x => x.Value);

            if (metadata.TryGetValue("STATUS", out var status) && string.Equals(status, "processing", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotAcceptableException($"File {message.Id} already processed");
            }

            await this._fileStorage.SetMetadataAsync(message.FileName, new Dictionary<string, string> { { "STATUS", "processing" } }, cancellationToken)
                                   .ConfigureAwait(false);

            this._logger.LogInformation("Starting result import");

            var content = await this._fileStorage.GetAsync(message.FileName, cancellationToken).ConfigureAwait(false);

            if (content.Length == 0)
            {
                throw new NotAcceptableException("Content is empty");
            }

            this._logger.LogInformation("Import file content uploaded. Uploaded {length} bytes.", content.Length);

            var list = new List<ResultCreateArgs>();

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

                const int expectedColCount = 12;
                if (colCount < expectedColCount)
                {
                    throw new NotAcceptableException($"Unexpected file format. The file should contain {expectedColCount} columns");
                }

                this._logger.LogInformation("Start reading excell data {rowsCount} rows.", rowCount.Value);

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

            if (list.Where(x => x.ParcelId != null).GroupBy(x => new { x.ParcelId, x.TaxYear }).Any(g => g.Count() > 1))
            {
                throw new NotAcceptableException($"Parcel Id with TaxYear should be unique for the event '{message.EventId}'");
            }

            if (list.Where(x => x.AdvertisementNumber != null).GroupBy(x => new { x.AdvertisementNumber, x.TaxYear }).Any(g => g.Count() > 1))
            {
                throw new NotAcceptableException($"Advertisement Number with TaxYear should be unique for the event '{message.EventId}'");
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
                    x => (x.AdvertisementNumber + x.TaxYear),
                    x => (x.AdvertisementNumber + x.TaxYear),
                    (r, d) => new
                    {
                        Result = r,
                        Delinquency = string.IsNullOrWhiteSpace(r.AdvertisementNumber) == false
                        ? d.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.AdvertisementNumber) == false)
                        : null,
                    })
                .GroupJoin(delinquencyList,
                    x => (x.Result.ParcelId + x.Result.TaxYear),
                    x => (x.ParcelId + x.TaxYear),
                    (r, d) => new { Result = r.Result, Delinquency = r.Delinquency ?? d.FirstOrDefault() })
                .ToList();

            this._logger.LogInformation("Delinquency/Results map created");

            list = null;

            const int maxBatchSize = 50000;

            var batches = new Dictionary<int, (List<CreateResultModel> CreateList, List<UpdateResultModel> UpdateList)>();

            var hasValuesToProcess = false;

            foreach (var (pair, index) in delinquencyMap.Select((x, i) => (x, i)))
            {
                var n = index / maxBatchSize;
                var batch = batches.TryGetValue(n, out var lists)
                    ? lists
                    : batches[n] = (CreateList: new List<CreateResultModel>(), UpdateList: new List<UpdateResultModel>());

                var delinquency = pair.Delinquency;

                if (delinquency == null)
                {
                    continue;
                }

                var row = pair.Result;

                if (delinquency.ResultId.HasValue == false)
                {
                    hasValuesToProcess = true;

                    batch.CreateList.Add(new CreateResultModel
                    {
                        Id = Guid.NewGuid(),
                        DelinquencyId = delinquency.Id,
                        BidNumber = row.BidNumber,
                        CertNo = row.CertNo,
                        TaxAmount = row.TaxAmount,
                        InterestRate = row.InterestRate,
                        Overbid = row.Overbid,
                        Premium = row.Premium,
                        PenaltyRate = row.PenaltyRate,
                        RecoverableFees = row.RecoverableFees,
                        NonRecoverableFees = row.NonRecoverableFees,
                    });
                }
                else
                {
                    hasValuesToProcess = true;

                    batch.UpdateList.Add(new UpdateResultModel
                    {
                        Id = delinquency.ResultId.Value,
                        DelinquencyId = delinquency.Id,
                        BidNumber = row.BidNumber,
                        CertNo = row.CertNo,
                        TaxAmount = row.TaxAmount,
                        InterestRate = row.InterestRate,
                        Overbid = row.Overbid,
                        Premium = row.Premium,
                        PenaltyRate = row.PenaltyRate,
                        RecoverableFees = row.RecoverableFees,
                        NonRecoverableFees = row.NonRecoverableFees,
                        CreatedOn = delinquency.ResultCreatedOn.Value,
                        CreatedById = delinquency.ResultCreatedById.Value,
                    });
                }
            }

            this._logger.LogInformation("{batchesCount} created.", batches.Count);

            if (hasValuesToProcess == false)
            {
                throw new NotAcceptableException("File does not contain appropriate records");
            }

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
                foreach (var (batch, index) in batches.Select((x, i) => (x, i)))
                {
                    this._logger.LogInformation("Start processing batch {index} of {batchesCount}.", index + 1, batches.Count);

                    var (createList, updateList) = batch.Value;

                    if (createList.Any())
                    {
                        this._logger.LogInformation("Start BulkCreateResultCommand for {cnt} items.", createList.Count);

                        await this._bulkCreateResultCommand
                            .DispatchAsync(createList, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                        this._logger.LogInformation("BulkCreateResultCommand for {cnt} items finished.", createList.Count);
                    }

                    if (updateList.Any())
                    {
                        this._logger.LogInformation("Start BulkUpdateCommand for {cnt} items.", updateList.Count);

                        await this._bulkUpdateResultCommand
                            .DispatchAsync(updateList, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                        this._logger.LogInformation("BulkUpdateCommand for {cnt} finished.", updateList.Count);
                    }

                    batch.Value.CreateList.Clear();
                    batch.Value.UpdateList.Clear();

                    progress += progressPerBatch;
                    await this._progressPublisher.PostProgressAsync((int)progress, cancellationToken).ConfigureAwait(false);

                    this._logger.LogInformation("Processed batch {index} of {batchesCount}.", index + 1, batches.Count);
                }

                batches = null;

                this._logger.LogInformation("Starting RefreshResultToBidRelationCommand.");

                await this._refreshResultToBidRelationCommand.DispatchAsync(new RefreshResultToBidRelationModel { Id = message.EventId }, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                this._logger.LogInformation("Finished RefreshResultToBidRelationCommand.");

                scope.Complete();

                this._logger.LogInformation("Transaction commited.");
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
    }
}
