using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Synergy.Common.Domain.Models.Extensions;
using Synergy.Common.Exceptions;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Extensions.Progress;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services
{
    public class EventDumpService :
        IMessageHandler<EventDumpFileCreateCommand>,
        IMessageHandler<RulesDumpFileCreateCommand>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly GetExportEventQuery _exportEventQuery;
        private readonly GetExportPropertiesQuery _exportPropertiesQuery;
        private readonly IProgressPublisher _progressPublisher;
        private readonly GetExportRulesQuery _exportRulesQuery;

        public EventDumpService(ILogger<EventDumpFileCreateCommand> logger,
                                IMapper mapper,
                                IFileStorage fileStorage,
                                GetExportEventQuery exportEventQuery,
                                GetExportPropertiesQuery exportPropertiesQuery,
                                IProgressPublisher progressPublisher,
                                GetExportRulesQuery exportRulesQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._exportEventQuery = exportEventQuery ?? throw new ArgumentNullException(nameof(exportEventQuery));
            this._exportPropertiesQuery = exportPropertiesQuery ?? throw new ArgumentNullException(nameof(exportPropertiesQuery));
            this._progressPublisher = progressPublisher;
            this._exportRulesQuery = exportRulesQuery ?? throw new ArgumentNullException(nameof(exportRulesQuery));
        }

        public void Handle(EventDumpFileCreateCommand message)
        {
            this.HandleAsync(message).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(EventDumpFileCreateCommand message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation("Data dump started.");

            var exportItem = await this._exportEventQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exportItem == null)
            {
                throw new NotFoundException();
            }

            await this._progressPublisher.PostProgressAsync(5, cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("ExportEventQuery finished.");

            var properties = await this._exportPropertiesQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("ExportPropertiesQuery finished.");

            if (properties?.Any() == false)
            {
                throw new NotAcceptableException("There are no records to process");
            }

            await this._progressPublisher.PostProgressAsync(15, cancellationToken).ConfigureAwait(false);

            var rows = properties.Select(x =>
            {
                var res = this._mapper.Map<EventDumpModel>(exportItem);
                this._mapper.Map(x, res);
                return res.ToDataDump();
            }).ToList();

            this._logger.LogInformation("Data dump dictionary created.");

            properties = null;
            exportItem = null;

            var columns = message.Fields.OrderBy(x => x.Order).ToList();

            if (rows.Count == 0)
            {
                throw new NotAcceptableException("There are no records to process");
            }

            await this._progressPublisher.PostProgressAsync(20, cancellationToken).ConfigureAwait(false);

            using (var package = new ExcelPackage())
            {
                this._logger.LogInformation("ExcelPackage created.");

                var worksheet = package.Workbook.Worksheets.Add("Dump");
                for (var i = 0; i < columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = string.IsNullOrWhiteSpace(columns[i].Alias) == true ? columns[i].Key : columns[i].Alias;
                }

                this._logger.LogInformation("Starting rows creation for {Count} rows.", rows.Count);

                var currentProgress = 20;
                var percentsPerBatch = (100 - currentProgress) / (rows.Count / 2000.0);

                for (var i = 0; i < rows.Count; i++)
                {
                    for (var j = 0; j < columns.Count; j++)
                    {
                        var key = columns[j].Key;
                        if (rows[i].ContainsKey(key) == false)
                        {
                            continue;
                        }

                        var val = rows[i][key];

                        var cell = worksheet.Cells[i + 2, j + 1];
                        cell.Value = val;

                        if (val is string)
                        {
                            cell.Style.Numberformat.Format = "@";
                        }
                        else if (val is DateTime)
                        {
                            cell.Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo?.ShortDatePattern;
                        }
                        else if (long.TryParse(Convert.ToString(val, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _) == true)
                        {
                            cell.Style.Numberformat.Format = "0";
                        }
                        else if (decimal.TryParse(Convert.ToString(val, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _) == true)
                        {
                            cell.Style.Numberformat.Format = "0.00";
                        }
                    }

                    if (i % 2000 == 0)
                    {
                        if (i != 0)
                        {
                            currentProgress = (int)(currentProgress + percentsPerBatch);

                            await this._progressPublisher.PostProgressAsync(currentProgress, cancellationToken).ConfigureAwait(false);
                        }

                        this._logger.LogInformation("Row {index} created. Rows count: {Count}.", rows.Count, i);
                    }
                }

                this._logger.LogInformation("Rows created. Rows count: {Count}.", rows.Count);

                var data = package.GetAsByteArray();

                this._logger.LogInformation("Start saving blob");

                await this._fileStorage.SaveAsync(data, message.FileName, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Handle(RulesDumpFileCreateCommand message)
        {
            this.HandleAsync(message).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(RulesDumpFileCreateCommand message, CancellationToken cancellationToken = default)
        {
            var rules = await this._exportRulesQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Rules");

                worksheet.Cells[1, 1].Value = "Rule Name";
                worksheet.Cells[1, 2].Value = "Checked";
                worksheet.Cells[1, 3].Value = "Result";

                for (int i = 0; i < rules.Count(); i++)
                {
                    worksheet.Cells[i + 2, 1].Value = rules.ElementAt(i).RuleName;
                    worksheet.Cells[i + 2, 2].Value = rules.ElementAt(i).Checked;
                    worksheet.Cells[i + 2, 3].Value = rules.ElementAt(i).Result;
                }

                var data = package.GetAsByteArray();

                await this._fileStorage.SaveAsync(data, message.FileName, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
