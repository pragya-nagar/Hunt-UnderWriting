using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Synergy.Common.Abstracts;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.Domain.Models.Extensions;
using Synergy.Common.Exceptions;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.DAL.Queries.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Attachment;
using Synergy.Underwriting.Models.EventAssignment;
using Synergy.Underwriting.Models.Rule;
using Event = Synergy.Underwriting.DAL.Queries.Entities.Event;
using EventModel = Synergy.Underwriting.Models.EventModel;
using RuleItemModel = Synergy.Underwriting.Models.Rule.RuleItemModel;

namespace Synergy.Underwriting.Domain
{
    public class EventService : IEventService
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        private readonly IQueryProvider<DAL.Queries.Entities.DataCutRule> _ruleQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventDataCutRule> _ruleEventQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.Event> _eventQueryProvider;

        private readonly IPublishMessage _serviceBus;
        private readonly IFileStorage _fileStorage;
        private readonly IGetEventsQuery _eventQuery;
        private readonly IGetEventAttachmentQuery _eventAttachmentQuery;
        private readonly IGetEventCalculatedFields _eventCalculatedFields;
        private readonly IMapper _mapper;

        private readonly IQueryProvider<EventDecisionLevel> _eventDecisionLevelQueryProvider;
        private readonly IGetEventAssigmentsCountQuery _getEventAssigmentsCountQuery;

        public EventService(ILogger<EventService> logger,
            ICurrentUserService currentUserService,
            IPublishMessage serviceBus,
            IFileStorage fileStorage,
            IGetEventsQuery eventQuery,
            IGetEventAttachmentQuery eventAttachmentQuery,
            IQueryProvider<EventDecisionLevel> eventDecisionLevelQueryProvider,
            IGetEventCalculatedFields eventCalculatedFields,
            IMapper mapper,
            IQueryProvider<DataCutRule> ruleQueryProvider,
            IQueryProvider<EventDataCutRule> ruleEventQueryProvider,
            IQueryProvider<Event> eventQueryProvider,
            IGetEventAssigmentsCountQuery getEventAssigmentsCountQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._eventQuery = eventQuery ?? throw new ArgumentNullException(nameof(eventQuery));
            this._eventAttachmentQuery = eventAttachmentQuery ?? throw new ArgumentNullException(nameof(eventAttachmentQuery));
            this._eventCalculatedFields = eventCalculatedFields ?? throw new ArgumentNullException(nameof(eventCalculatedFields));
            this._mapper = mapper;
            this._ruleQueryProvider = ruleQueryProvider ?? throw new ArgumentNullException(nameof(ruleQueryProvider));
            this._ruleEventQueryProvider = ruleEventQueryProvider ?? throw new ArgumentNullException(nameof(ruleEventQueryProvider));
            this._eventQueryProvider = eventQueryProvider ?? throw new ArgumentNullException(nameof(eventQueryProvider));

            this._eventDecisionLevelQueryProvider = eventDecisionLevelQueryProvider ?? throw new ArgumentNullException(nameof(eventDecisionLevelQueryProvider));
            this._getEventAssigmentsCountQuery = getEventAssigmentsCountQuery ?? throw new ArgumentNullException(nameof(eventDecisionLevelQueryProvider));
        }

        public async Task<SearchResultModel<EventModel>> GetListAsync(SearchArgsModel<EventFilterArgs, EventSortField> args, CancellationToken cancellationToken = default)
        {
            var query = this._eventQuery;

            if (string.IsNullOrWhiteSpace(args?.FullSearch) == false)
            {
                var val = args.FullSearch.Trim();
                query.Search(val);
            }

            if (args?.Filter?.Type != null)
            {
                var val = (int)args.Filter.Type.Value;
                query.FilterByEventType(val);
            }

            if (args?.Filter?.StateId != null)
            {
                var val = args.Filter.StateId.Value;
                query.FilterByState(val);
            }

            if (args?.Filter?.AssignedTo != null)
            {
                var val = args.Filter.AssignedTo.Value;
                query.FindByUserId(val);
            }

            if (args?.Filter?.IsLockedStatus != null)
            {
                var val = args.Filter.IsLockedStatus.Value;
                query.FilterByStatus(val);
            }

            if (args?.SortField != null)
            {
                query = (args.SortOrder ?? SortOrder.Asc) == SortOrder.Asc
                    ? query.OrderBy(args.SortField.Value)
                    : query.OrderByDescending(args.SortField.Value);
            }

            query.Skip(args?.Offset ?? 0).Take(args?.Limit ?? 50);

            var items = await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false);

            var count = query.TotalCount ?? 0;

            return new SearchResultModel<EventModel>
            {
                TotalCount = count,
                List = this._mapper.Map<IEnumerable<EventModel>>(items),
            };
        }

        public async Task<EventDetailsModel> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var items = await this._eventQuery.IncludeAttachments().FindById(id).ExeсuteAsync(cancellationToken).ConfigureAwait(false);

            var item = items.FirstOrDefault();
            var eventData = item == null ? throw new NotFoundException() : this._mapper.Map<EventDetailsModel>(item);

            return eventData;
        }

        public async Task<EventCalculatedFieldsModel> FindEventCalculatedData(Guid id, CancellationToken cancellationToken = default)
        {
            IEnumerable<DAL.Queries.Original.Models.EventCalculatedFieldsModel> eventCalculatedItems = await this._eventCalculatedFields.FilterByEventId(id).ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            var eventCalculatedFields = this._mapper.Map<EventCalculatedFieldsModel>(eventCalculatedItems.FirstOrDefault());

            return eventCalculatedFields;
        }

        public async Task<ObjectAccessModel> FindAttachmentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _eventAttachmentQuery.FindById(id).ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            if (item == null)
            {
                throw new NotFoundException();
            }

            var uploadUrl = await this._fileStorage.GetAccessAsync(item.Path, cancellationToken).ConfigureAwait(false);
            var access = uploadUrl.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(access?.Url))
            {
                throw new NotFoundException();
            }

            return await Task.FromResult(access).ConfigureAwait(false);
        }

        public async Task DeleteAttachmentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _eventAttachmentQuery.FindById(id).ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            if (item == null)
            {
                throw new NotFoundException();
            }

            if (this._currentUserService.UserId != item.CreatedBy.Id)
            {
                throw new NotAcceptableException();
            }

            var command = Command.Create<EventAttachmentDeleteCommand>(id, this._currentUserService.UserId);

            await _serviceBus.PublishAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<EventAssignmentResult> GetLevelListAsync(Guid id, CancellationToken cancellationToken = default)
        {
           IGetEventAssigmentsCountQuery query = _getEventAssigmentsCountQuery.FindById(id);

           return _mapper.Map<EventAssignmentResult>(await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false));
        }

        public async Task<IEnumerable<string>> GetDumpFieldsAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var levelCount = await this._eventDecisionLevelQueryProvider.Query
                .CountAsync(x => x.DeletedOn == null && x.IsFinal == false && x.EventId == eventId, cancellationToken)
                .ConfigureAwait(false);

            return new EventDumpModel(2, levelCount).ToDataDump().Keys;
        }

        public async Task SetLockStatusAsync(Guid id, CancellationToken cancellationToken)
        {
            var command = Command.Create<SetEventLockStatusCommand>(id, this._currentUserService.UserId);
            this._mapper.Map(id, command);
            await this._serviceBus.PublishAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SearchResultModel<RuleModel>> GetRuleListAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var countyId = await this._eventQueryProvider.Query.Where(x => x.Id == id).Select(x => x.CountyId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (countyId == 0)
            {
                return new SearchResultModel<RuleModel>()
                {
                    TotalCount = 0,
                    List = Enumerable.Empty<RuleModel>(),
                };
            }

            var query = from r in this._ruleQueryProvider.Query
                        join re in this._ruleEventQueryProvider.Query.Where(x => x.EventDataCutStrategy.EventId == id && x.EventDataCutStrategy.IsActive == true) on r.Id equals re.DataCutRuleId into left
                        where r.CountyId == countyId
                        from re in left.DefaultIfEmpty()
                        select new RuleModel
                        {
                            Id = r.Id,
                            DataCutResultType = new FastEntityModel<int>()
                            {
                                Id = r.DataCutResultTypeId,
                                Name = r.DataCutResultType.Description,
                            },
                            IsAttached = re != null ? true : false,
                            Name = r.Name,
                            DataCutRuleItems = r.DataCutRuleItems.Select(x => new RuleItemModel
                            {
                                Value = x.Value,
                                DataCutLogicType = new FastEntityModel<int>()
                                {
                                    Id = x.DataCutLogicTypeId,
                                    Name = x.DataCutLogicType.Description,
                                },
                                DataCutRuleField = new FastEntityModel<int>()
                                {
                                    Id = x.DataCutRuleFieldId,
                                    Name = x.DataCutRuleField.Description,
                                },
                            }),
                        };
            var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new SearchResultModel<RuleModel>()
            {
                List = list,
                TotalCount = list.Count,
            };
        }

        public async Task<FileId> CreateRulesDumpAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            string eventName = await this._eventQueryProvider.Query.Where(x => x.Id == id).Select(x => x.EventNumber)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            eventName = Regex.Replace(eventName, @"\s+", "-").Replace("#", string.Empty);

            FileId exportId = FileId.Generate(id, "RulesExport", $"{eventName}-Strategy-Configuration-Rules.xlsx");
            RulesDumpFileCreateCommand command = Command.Create<RulesDumpFileCreateCommand>(Guid.NewGuid(), this._currentUserService.UserId);

            command.EventId = id;
            command.FileName = exportId.FileName;

            await this._serviceBus.PublishAsync(command, cancellationToken).ConfigureAwait(false);

            return exportId;
        }
    }
}
