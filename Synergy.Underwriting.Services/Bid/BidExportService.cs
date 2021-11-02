using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services
{
    public class BidExportService : IMessageHandler<BidExportFileCreateCommand>
    {
        private readonly ILogger<BidService> _logger;
        private readonly IFileStorage _fileStorage;
        private readonly GetBidListQuery _getBidListQuery;

        public BidExportService(ILogger<BidService> logger, IFileStorage fileStorage, GetBidListQuery getBidListQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._getBidListQuery = getBidListQuery ?? throw new ArgumentNullException(nameof(getBidListQuery));
        }

        public void Handle(BidExportFileCreateCommand message)
        {
            this.HandleAsync(message).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(BidExportFileCreateCommand message, CancellationToken cancellationToken = default)
        {
            var list = await this._getBidListQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Bids");

                worksheet.Cells[1, 1].Value = "Bidder Number";
                worksheet.Cells[1, 2].Value = "Purchasing Entity";
                worksheet.Cells[1, 3].Value = "Portfolio";

                foreach (var (value, index) in list.Values.Select((x, i) => (x, i)))
                {
                    worksheet.Cells[index + 2, 1].Value = value.Number;
                    worksheet.Cells[index + 2, 2].Value = value.Entity;
                    worksheet.Cells[index + 2, 3].Value = value.Portfolio;
                }

                var data = package.GetAsByteArray();

                await this._fileStorage.SaveAsync(data, message.FileName, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}