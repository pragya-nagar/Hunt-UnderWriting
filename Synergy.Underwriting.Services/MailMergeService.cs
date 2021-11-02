using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Synergy.Common.Exceptions;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Extensions.Progress;
using Synergy.ServiceBus.Messages;
using Synergy.Underwriting.DAL.Commands.Models.Results.MailMerge;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services
{
    public class MailMergeService : IMessageHandler<MailMergeCommand>, IMessageHandler<MailMergeFinishedEvent>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPublishMessage _publisher;
        private readonly IProgressPublisher _progressPublisher;
        private readonly IFileStorage _fileStorage;

        private readonly GetMailMergePropertyFieldsQuery _mailMergePropertyFieldsQuery;
        private readonly GetMailMergeEventFieldsQuery _mailMergeEventFieldsQuery;
        private readonly GetMailMergeTemplateQuery _mailMergeTemplateQuery;

        public MailMergeService(ILogger<MailMergeService> logger,
                                IMapper mapper,
                                IPublishMessage publisher,
                                IProgressPublisher progressPublisher,
                                IFileStorage fileStorage,
                                GetMailMergePropertyFieldsQuery mailMergePropertyFieldsQuery,
                                GetMailMergeEventFieldsQuery mailMergeEventFieldsQuery,
                                GetMailMergeTemplateQuery mailMergeTemplateQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._mailMergePropertyFieldsQuery = mailMergePropertyFieldsQuery ?? throw new ArgumentNullException(nameof(mailMergePropertyFieldsQuery));
            this._mailMergeEventFieldsQuery = mailMergeEventFieldsQuery ?? throw new ArgumentNullException(nameof(mailMergeEventFieldsQuery));
            this._mailMergeTemplateQuery = mailMergeTemplateQuery ?? throw new ArgumentNullException(nameof(mailMergeTemplateQuery));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._progressPublisher = progressPublisher ?? throw new ArgumentNullException(nameof(progressPublisher));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public void Handle(MailMergeCommand message)
        {
            this.HandleAsync(message, new CancellationToken(false)).Wait();
        }

        public async Task HandleAsync(MailMergeCommand message, CancellationToken cancellationToken)
        {
            var delinquencyIds = await GetDelinquencyIdList(message.DeliquencyPath, cancellationToken).ConfigureAwait(false);
            if (delinquencyIds.Any() == false)
            {
                throw new NotAcceptableException("Required delinquency list is empty");
            }

            var template = await this._mailMergeTemplateQuery.ExecuteAsync(message.TemplateId, cancellationToken).ConfigureAwait(false);
            if (template == null)
            {
                throw new NotAcceptableException("Merge template not found");
            }

            if (string.IsNullOrEmpty(template.FileId))
            {
                throw new NotAcceptableException("Merge template file id not found");
            }

            var propertyFields = await this._mailMergePropertyFieldsQuery.ExecuteAsync((delinquencyIds, message.EventId), cancellationToken).ConfigureAwait(false);
            if (propertyFields.Count() != delinquencyIds.Count)
            {
                throw new NotAcceptableException("Please double-check Internal delinquency Id’s. Some of them are not relevant.");
            }

            var @event = await this._mailMergeEventFieldsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            var mergeSingleFieldsList = propertyFields.Select(x =>
            {
                var mergeFields = this._mapper.Map<MailMergePropertyModel, MergeSingleFields>(x);
                this._mapper.Map(@event, mergeFields);
                return mergeFields;
            }).ToList();

            var mergeFieldsList = new List<MergeFields>();
            if (template.GroupingType == (int)MergeFieldsGroupingType.PerOwner)
            {
                var groups = mergeSingleFieldsList.GroupBy(x => x.Owner);
                mergeFieldsList = groups.Select(g => this._mapper.Map<MergeFields>(g.ToList())).ToList();
            }
            else
            {
                var groups = mergeSingleFieldsList.GroupBy(x => x.InternalDelinquencyId);
                mergeFieldsList = groups.Select(g => this._mapper.Map<MergeFields>(g.ToList())).ToList();
            }

            var evt = ServiceBus.Abstracts.Event.Create<MailMergeStartedEvent>(message.Id, message.CreatedBy);
            evt.ResultPath = message.ResultPath;
            evt.TemplateFilePath = template.FileId.Replace(':', '/');
            evt.MergeFields = mergeFieldsList;
            evt.Source = "Underwriting";
            await this._publisher.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(MailMergeFinishedEvent message)
        {
            this.HandleAsync(message, default).Wait();
        }

        public async Task HandleAsync(MailMergeFinishedEvent message, CancellationToken cancellationToken)
        {
            if (message.Source == "Underwriting")
            {
                var evt = ServiceBus.Abstracts.Event.Create<OperationStatusEvent>(Guid.NewGuid(), message.CreatedBy);
                evt.Code = HttpStatusCode.OK;
                evt.Message = "OK";

                await this._publisher.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<List<Guid>> GetDelinquencyIdList(string deliquencyPath, CancellationToken cancellationToken)
        {
            var delinquencyIds = new List<Guid>();
            var fileContent = await this._fileStorage.GetAsync(deliquencyPath, cancellationToken).ConfigureAwait(false);
            using (var memoryStream = new MemoryStream(fileContent))
            using (var package = new ExcelPackage(memoryStream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    throw new NotAcceptableException("Content is empty");
                }

                var rowCount = worksheet.Dimension?.Rows;

                if (rowCount == null)
                {
                    throw new NotAcceptableException("Content is empty");
                }

                int startRow = 1;
                var firstPlainId = worksheet.Cells[startRow, 1].Value;
                if (firstPlainId != null && Guid.TryParse(firstPlainId.ToString(), out Guid firstParsedId) == true)
                {
                    delinquencyIds.Add(firstParsedId);
                }

                startRow = 2;
                for (var rowIndex = startRow; rowIndex <= rowCount.Value; ++rowIndex)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var plainId = worksheet.Cells[rowIndex, 1].Value;
                    if (plainId == null)
                    {
                        continue;
                    }

                    if (Guid.TryParse(plainId.ToString(), out Guid parsedId) == false)
                    {
                        throw new NotAcceptableException($"Delinquency id {plainId} has unknown format");
                    }

                    delinquencyIds.Add(parsedId);
                }
            }

            return delinquencyIds;
        }
    }
}
