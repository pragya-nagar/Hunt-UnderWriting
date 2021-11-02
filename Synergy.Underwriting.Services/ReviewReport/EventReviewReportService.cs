using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Extensions.Progress;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands.Event;

namespace Synergy.Underwriting.Services.ReviewReport
{
    public class EventReviewReportService :
        IMessageHandler<ReviewDumpCreateCommand>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly IProgressPublisher _progressPublisher;
        private readonly GetEventReviewReportQuery _getEventReviewReportQuery;
        private readonly GetEventPerUserReviewReportQuery _getEventPerUserReviewReportQuery;

        public EventReviewReportService(ILogger<ReviewDumpCreateCommand> logger,
                                IMapper mapper,
                                IFileStorage fileStorage,
                                GetEventReviewReportQuery getEventReviewReportQuery,
                                GetEventPerUserReviewReportQuery getEventPerUserReviewReportQuery,
                                IProgressPublisher progressPublisher)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._progressPublisher = progressPublisher;
            this._getEventReviewReportQuery = getEventReviewReportQuery ?? throw new ArgumentNullException(nameof(getEventReviewReportQuery));
            this._getEventPerUserReviewReportQuery = getEventPerUserReviewReportQuery ?? throw new ArgumentNullException(nameof(getEventPerUserReviewReportQuery));
        }

        public void Handle(ReviewDumpCreateCommand message)
        {
            this.HandleAsync(message).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(ReviewDumpCreateCommand message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"Review dump started.");

            await this._progressPublisher.PostProgressAsync(5, cancellationToken).ConfigureAwait(false);

            using (var package = new ExcelPackage())
            {
                if (message.IsPerUserReport)
                {
                    IEnumerable<EventPerUserReviewReportModel> report = await this._getEventPerUserReviewReportQuery.ExecuteAsync(message, cancellationToken).ConfigureAwait(false);
                    GenerateEventPerUserReviewReport(package, report);
                }
                else
                {
                    IEnumerable<EventReviewReportModel> reportData = await this._getEventReviewReportQuery.ExecuteAsync(message, cancellationToken).ConfigureAwait(false);
                    GenerateEventReviewReport(package, reportData);
                }

                var data = package.GetAsByteArray();

                await this._fileStorage.SaveAsync(data, message.FileName, cancellationToken).ConfigureAwait(false);
            }
        }

        private void GenerateEventReviewReport(ExcelPackage package, IEnumerable<EventReviewReportModel> reportData)
        {
            var worksheet = package.Workbook.Worksheets.Add("Review");

            worksheet.Cells[1, 1].Value = "County";
            worksheet.Cells[1, 2].Value = "Event";
            worksheet.Cells[1, 3].Value = "Total";
            worksheet.Cells[1, 4].Value = "Bulk Rejected";
            worksheet.Cells[1, 5].Value = "Bulk Approved";
            worksheet.Cells[1, 6].Value = "Review Available";
            worksheet.Cells[1, 7].Value = "Assigned";
            worksheet.Cells[1, 8].Value = "Approved";
            worksheet.Cells[1, 9].Value = "Disapproved";
            worksheet.Cells[1, 10].Value = "Research";
            worksheet.Cells[1, 11].Value = "Unreviewed";
            worksheet.Cells[1, 12].Value = "Review Completed";
            worksheet.Cells[1, 13].Value = "Percent Unreviewed";
            worksheet.Cells[1, 14].Value = "Percent Reviewed";
            worksheet.Cells[1, 15].Value = "Percent Auto Decision";
            worksheet.Cells[1, 16].Value = "Percent Assigned";
            worksheet.Cells[1, 17].Value = "Percent Unassigned";

            for (int i = 0; i < reportData.Count(); i++)
            {
                worksheet.Cells[i + 2, 1].Value = reportData.ElementAt(i).County;
                worksheet.Cells[i + 2, 2].Value = reportData.ElementAt(i).EventNumber;
                worksheet.Cells[i + 2, 3].Value = reportData.ElementAt(i).TotalCount;
                worksheet.Cells[i + 2, 3].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 4].Value = reportData.ElementAt(i).BulkRejected;
                worksheet.Cells[i + 2, 4].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 5].Value = reportData.ElementAt(i).BulkApproved;
                worksheet.Cells[i + 2, 5].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 6].Value = reportData.ElementAt(i).ReviewAvailable;
                worksheet.Cells[i + 2, 6].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 7].Value = reportData.ElementAt(i).Assigned;
                worksheet.Cells[i + 2, 7].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 8].Value = reportData.ElementAt(i).Approved;
                worksheet.Cells[i + 2, 8].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 9].Value = reportData.ElementAt(i).Disapproved;
                worksheet.Cells[i + 2, 9].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 10].Value = reportData.ElementAt(i).Research;
                worksheet.Cells[i + 2, 10].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 11].Value = reportData.ElementAt(i).Unreviewed;
                worksheet.Cells[i + 2, 11].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 12].Value = reportData.ElementAt(i).ReviewsCompleted;
                worksheet.Cells[i + 2, 12].Style.Numberformat.Format = "0";
                worksheet.Cells[i + 2, 13].Value = reportData.ElementAt(i).UnreviewedPercent;
                worksheet.Cells[i + 2, 13].Style.Numberformat.Format = "0.00";
                worksheet.Cells[i + 2, 14].Value = reportData.ElementAt(i).ReviewedPercent;
                worksheet.Cells[i + 2, 14].Style.Numberformat.Format = "0.00";
                worksheet.Cells[i + 2, 15].Value = reportData.ElementAt(i).AutoDecisionPercent;
                worksheet.Cells[i + 2, 15].Style.Numberformat.Format = "0.00";
                worksheet.Cells[i + 2, 16].Value = reportData.ElementAt(i).AssignedPercent;
                worksheet.Cells[i + 2, 16].Style.Numberformat.Format = "0.00";
                worksheet.Cells[i + 2, 17].Value = reportData.ElementAt(i).UnassignedPercent;
                worksheet.Cells[i + 2, 17].Style.Numberformat.Format = "0.00";
            }
        }

        private void GenerateEventPerUserReviewReport(ExcelPackage package, IEnumerable<EventPerUserReviewReportModel> reportData)
        {
            var worksheet = package.Workbook.Worksheets.Add("Review");

            worksheet.Cells[1, 1].Value = "Reviewer";
            worksheet.Cells[1, 2].Value = "Level";
            worksheet.Cells[1, 3].Value = "County";
            worksheet.Cells[1, 4].Value = "Event";
            worksheet.Cells[1, 5].Value = "Assigned";
            worksheet.Cells[1, 6].Value = "Approved";
            worksheet.Cells[1, 7].Value = "Disapproved";
            worksheet.Cells[1, 8].Value = "Research";
            worksheet.Cells[1, 9].Value = "Unreviewed";
            worksheet.Cells[1, 10].Value = "Review Completed";
            worksheet.Cells[1, 11].Value = "Percent Unreviewed";
            worksheet.Cells[1, 12].Value = "Percent Reviewed";

            reportData = reportData.Where(rd => rd.Assigned > 0).OrderByDescending(rd => rd.EventNumber).ThenBy(rd => rd.ReviewerFirstName).ThenBy(rd => rd.ReviewerLastName).ThenBy(rd => rd.LevelOrder);
            for (int i = 0; i < reportData.Count(); i++)
            {
                worksheet.Cells[i + 2, 1].Value = reportData.ElementAt(i).ReviewerFirstName + " " + reportData.ElementAt(i).ReviewerLastName;
                worksheet.Cells[i + 2, 2].Value = reportData.ElementAt(i).Level;
                worksheet.Cells[i + 2, 3].Value = reportData.ElementAt(i).County;
                worksheet.Cells[i + 2, 4].Value = reportData.ElementAt(i).EventNumber;

                worksheet.Cells[i + 2, 5].Value = reportData.ElementAt(i).Assigned;
                worksheet.Cells[i + 2, 5].Style.Numberformat.Format = "0";

                worksheet.Cells[i + 2, 6].Value = reportData.ElementAt(i).Approved;
                worksheet.Cells[i + 2, 6].Style.Numberformat.Format = "0";

                worksheet.Cells[i + 2, 7].Value = reportData.ElementAt(i).Disapproved;
                worksheet.Cells[i + 2, 7].Style.Numberformat.Format = "0";

                worksheet.Cells[i + 2, 8].Value = reportData.ElementAt(i).Research;
                worksheet.Cells[i + 2, 8].Style.Numberformat.Format = "0";

                worksheet.Cells[i + 2, 9].Value = reportData.ElementAt(i).Unreviewed;
                worksheet.Cells[i + 2, 9].Style.Numberformat.Format = "0";

                worksheet.Cells[i + 2, 10].Value = reportData.ElementAt(i).ReviewsCompleted;
                worksheet.Cells[i + 2, 10].Style.Numberformat.Format = "0";

                worksheet.Cells[i + 2, 11].Value = reportData.ElementAt(i).UnreviewedPercent;
                worksheet.Cells[i + 2, 11].Style.Numberformat.Format = "0.00";

                worksheet.Cells[i + 2, 12].Value = reportData.ElementAt(i).ReviewedPercent;
                worksheet.Cells[i + 2, 12].Style.Numberformat.Format = "0.00";
            }
        }
    }
}
