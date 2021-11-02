using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Synergy.Common.Abstracts;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.Exceptions;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.DAL.Queries.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Address;
using Synergy.Underwriting.Models.Commands.Attachment;
using Synergy.Underwriting.Models.Property;
using AuctionType = Synergy.Underwriting.DAL.Queries.Entities.AuctionType;
using EventType = Synergy.Underwriting.DAL.Queries.Entities.EventType;

namespace Synergy.Underwriting.Domain
{
    public class PropertyService : IPropertyService
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClockService _clockService;

        private readonly IPublishMessage _serviceBus;
        private readonly IFileStorage _fileStorage;

        private readonly IQueryProvider<DAL.Queries.Entities.Event> _eventQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventAudit> _eventAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.State> _stateQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.StateAudit> _stateAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.County> _countyQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.CountyAudit> _countyAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.Property> _propertyQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyAudit> _propertyAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyValuation> _propertyValuationQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyValuationAudit> _propertyValuationAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DelinquencyPropertyScoring> _delinquencyPropertyScoringQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DelinquencyPropertyScoringAudit> _delinquencyPropertyScoringAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.Lead> _leadQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.LeadAudit> _leadAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.Delinquency> _delinquencyQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DelinquencyAudit> _delinquencyAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.SupplementalData> _supplementalDataQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.SupplementalDataAudit> _supplementalDataAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DelinquencyPropertyDisplayStrategy> _delinquencyPropertyDisplayStrategyQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DelinquencyPropertyDisplayStrategyAudit> _delinquencyPropertyDisplayStrategyAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.Decision> _decisionQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DecisionAudit> _decisionAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventType> _eventTypeQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventTypeAudit> _eventTypeAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.AuctionType> _auctionTypeQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.AuctionTypeAudit> _auctionTypeAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventDataCutDecision> _eventDataCutDecisionQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventDataCutDecisionAudit> _eventDataCutDecisionAuditQueryProvider;

        private readonly IQueryProvider<DAL.Queries.Entities.User> _userQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventDecisionLevel> _eventDecisionLevelQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.EventDataCutStrategy> _eventDataCutStrategy;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyAttachment> _propertyAttachmentQueryProvider;

        private readonly IQueryProvider<DelinquencyComment> _commentQueryProvider;

        private readonly IGetStateTaxRatesQuery _stateTaxRatesQuery;
        private readonly IGetDelinquencyCommentsQuery _delinquencyCommentsQuery;
        private readonly IGetEventDelinquenciesQuery _eventDelinquenciesQuery;
        private readonly IGetEventDecisionLevelQuery _eventDecisionLevelQuery;

        private readonly IMapper _mapper;

        public PropertyService(ILogger<EventService> logger,
            ICurrentUserService currentUserService,
            IClockService clockService,
            IPublishMessage serviceBus,
            IFileStorage fileStorage,
            IQueryProvider<DAL.Queries.Entities.PropertyAttachment> propertyAttachmentQueryProvider,
            IGetDelinquencyCommentsQuery delinquencyCommentsQuery,
            IGetStateTaxRatesQuery stateTaxRatesQuery,
            IGetEventDelinquenciesQuery eventDelinquenciesQuery,
            IGetEventDecisionLevelQuery eventDecisionLevelQuery,
            IMapper mapper,
            IQueryProvider<DAL.Queries.Entities.Event> eventQueryProvider,
            IQueryProvider<EventAudit> eventAuditQueryProvider,
            IQueryProvider<Delinquency> delinquencyQueryProvider,
            IQueryProvider<DelinquencyAudit> delinquencyAuditQueryProvider,
            IQueryProvider<Property> propertyQueryProvider,
            IQueryProvider<PropertyAudit> propertyAuditQueryProvider,
            IQueryProvider<PropertyValuation> propertyValuationQueryProvider,
            IQueryProvider<PropertyValuationAudit> propertyValuationAuditQueryProvider,
            IQueryProvider<DelinquencyPropertyScoring> delinquencyPropertyScoringQueryProvider,
            IQueryProvider<DelinquencyPropertyScoringAudit> delinquencyPropertyScoringAuditQueryProvider,
            IQueryProvider<State> stateQueryProvider,
            IQueryProvider<StateAudit> stateAuditQueryProvider,
            IQueryProvider<County> countyQueryProvider,
            IQueryProvider<CountyAudit> countyAuditQueryProvider,
            IQueryProvider<Lead> leadQueryProvider,
            IQueryProvider<LeadAudit> leadAuditQueryProvider,
            IQueryProvider<SupplementalData> supplementalDataQueryProvider,
            IQueryProvider<SupplementalDataAudit> supplementalDataAuditQueryProvider,
            IQueryProvider<DelinquencyPropertyDisplayStrategy> delinquencyPropertyDisplayStrategyQueryProvider,
            IQueryProvider<DelinquencyPropertyDisplayStrategyAudit> delinquencyPropertyDisplayStrategyAuditQueryProvider,
            IQueryProvider<Decision> decisionQueryProvider,
            IQueryProvider<DecisionAudit> decisionAuditQueryProvider,
            IQueryProvider<EventType> eventTypeQueryProvider,
            IQueryProvider<EventTypeAudit> eventTypeAuditQueryProvider,
            IQueryProvider<AuctionType> auctionTypeQueryProvider,
            IQueryProvider<AuctionTypeAudit> auctionTypeAuditQueryProvider,
            IQueryProvider<EventDataCutDecision> eventDataCutDecisionQueryProvider,
            IQueryProvider<EventDataCutDecisionAudit> eventDataCutDecisionAuditQueryProvider,
            IQueryProvider<User> userQueryProvider,
            IQueryProvider<EventDecisionLevel> eventDecisionLevelQueryProvider,
            IQueryProvider<EventDataCutStrategy> eventDataCutStrategy,
            IQueryProvider<DelinquencyComment> commentQueryProvider)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            this._clockService = clockService ?? throw new ArgumentNullException(nameof(clockService));

            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));

            this._stateTaxRatesQuery = stateTaxRatesQuery ?? throw new ArgumentNullException(nameof(stateTaxRatesQuery));
            this._delinquencyCommentsQuery = delinquencyCommentsQuery ?? throw new ArgumentNullException(nameof(delinquencyCommentsQuery));
            this._eventDelinquenciesQuery = eventDelinquenciesQuery ?? throw new ArgumentNullException(nameof(eventDelinquenciesQuery));
            this._eventDecisionLevelQuery = eventDecisionLevelQuery ?? throw new ArgumentNullException(nameof(eventDecisionLevelQuery));

            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._leadQueryProvider = leadQueryProvider ?? throw new ArgumentNullException(nameof(leadQueryProvider));
            this._leadAuditQueryProvider = leadAuditQueryProvider ?? throw new ArgumentNullException(nameof(leadAuditQueryProvider));
            this._eventQueryProvider = eventQueryProvider ?? throw new ArgumentNullException(nameof(eventQueryProvider));
            this._eventAuditQueryProvider = eventAuditQueryProvider ?? throw new ArgumentNullException(nameof(eventAuditQueryProvider));
            this._propertyQueryProvider = propertyQueryProvider ?? throw new ArgumentNullException(nameof(propertyQueryProvider));
            this._propertyAuditQueryProvider = propertyAuditQueryProvider ?? throw new ArgumentNullException(nameof(propertyAuditQueryProvider));
            this._propertyValuationQueryProvider = propertyValuationQueryProvider ?? throw new ArgumentNullException(nameof(propertyValuationQueryProvider));
            this._propertyValuationAuditQueryProvider = propertyValuationAuditQueryProvider ?? throw new ArgumentNullException(nameof(propertyValuationAuditQueryProvider));
            this._delinquencyPropertyScoringQueryProvider = delinquencyPropertyScoringQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyPropertyScoringQueryProvider));
            this._delinquencyPropertyScoringAuditQueryProvider = delinquencyPropertyScoringAuditQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyPropertyScoringAuditQueryProvider));
            this._delinquencyQueryProvider = delinquencyQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyQueryProvider));
            this._delinquencyAuditQueryProvider = delinquencyAuditQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyAuditQueryProvider));
            this._countyQueryProvider = countyQueryProvider ?? throw new ArgumentNullException(nameof(countyQueryProvider));
            this._countyAuditQueryProvider = countyAuditQueryProvider ?? throw new ArgumentNullException(nameof(countyAuditQueryProvider));
            this._stateQueryProvider = stateQueryProvider ?? throw new ArgumentNullException(nameof(stateQueryProvider));
            this._stateAuditQueryProvider = stateAuditQueryProvider ?? throw new ArgumentNullException(nameof(stateAuditQueryProvider));
            this._supplementalDataQueryProvider = supplementalDataQueryProvider ?? throw new ArgumentNullException(nameof(supplementalDataQueryProvider));
            this._supplementalDataAuditQueryProvider = supplementalDataAuditQueryProvider ?? throw new ArgumentNullException(nameof(supplementalDataAuditQueryProvider));
            this._delinquencyPropertyDisplayStrategyQueryProvider = delinquencyPropertyDisplayStrategyQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyPropertyDisplayStrategyQueryProvider));
            this._delinquencyPropertyDisplayStrategyAuditQueryProvider = delinquencyPropertyDisplayStrategyAuditQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyPropertyDisplayStrategyAuditQueryProvider));
            this._decisionQueryProvider = decisionQueryProvider ?? throw new ArgumentNullException(nameof(decisionQueryProvider));
            this._decisionAuditQueryProvider = decisionAuditQueryProvider ?? throw new ArgumentNullException(nameof(decisionAuditQueryProvider));
            this._eventTypeQueryProvider = eventTypeQueryProvider ?? throw new ArgumentNullException(nameof(eventTypeQueryProvider));
            this._eventTypeAuditQueryProvider = eventTypeAuditQueryProvider ?? throw new ArgumentNullException(nameof(eventTypeAuditQueryProvider));
            this._auctionTypeQueryProvider = auctionTypeQueryProvider ?? throw new ArgumentNullException(nameof(auctionTypeQueryProvider));
            this._auctionTypeAuditQueryProvider = auctionTypeAuditQueryProvider ?? throw new ArgumentNullException(nameof(auctionTypeAuditQueryProvider));
            this._eventDataCutDecisionQueryProvider = eventDataCutDecisionQueryProvider ?? throw new ArgumentNullException(nameof(eventDataCutDecisionQueryProvider));
            this._eventDataCutDecisionAuditQueryProvider = eventDataCutDecisionAuditQueryProvider ?? throw new ArgumentNullException(nameof(eventDataCutDecisionAuditQueryProvider));

            this._userQueryProvider = userQueryProvider ?? throw new ArgumentNullException(nameof(userQueryProvider));
            this._eventDecisionLevelQueryProvider = eventDecisionLevelQueryProvider ?? throw new ArgumentNullException(nameof(eventDecisionLevelQueryProvider));
            this._eventDataCutStrategy = eventDataCutStrategy ?? throw new ArgumentNullException(nameof(eventDataCutStrategy));
            this._propertyAuditQueryProvider = propertyAuditQueryProvider ?? throw new ArgumentNullException(nameof(propertyAuditQueryProvider));
            this._commentQueryProvider = commentQueryProvider ?? throw new ArgumentNullException(nameof(commentQueryProvider));
            this._propertyAttachmentQueryProvider = propertyAttachmentQueryProvider ?? throw new ArgumentNullException(nameof(propertyAttachmentQueryProvider));
        }

        public async Task<SearchResultModel<PropertyAssignmentModel>> GetListAsync(SearchArgsModel<PropertyFilterArgs, PropertySortField> args, CancellationToken cancellationToken = default)
        {
            int count;
            IEnumerable<DAL.Queries.Original.Models.PropertyAssignmentModel> items;

            var eventId = args?.Filter?.EventId;

            var query = this._eventDelinquenciesQuery
                .IncludeLead()
                .IncludeValuation()
                .IncludeSupplementalEventData();

            if (eventId.HasValue)
            {
                query.FilterByEventIds(new List<Guid> { eventId.Value });
            }

            if (string.IsNullOrWhiteSpace(args?.FullSearch) == false)
            {
                var val = args.FullSearch.Trim();
                query.Search(val);
            }

            var userId = Guid.Empty;
            if (args?.Filter?.AssignmentByUser.HasValue == true && args?.Filter?.AssignmentByUser.Value == true)
            {
                userId = this._currentUserService.UserId;
                query.FindByUserId(userId).FilterByInactiveDataCut();
            }

            query.FilterByPropertyFields(this._mapper.Map<DAL.Queries.Original.Models.PropertyFieldsFilterModel>(args.Filter));

            var levelId = args?.Filter?.LevelId;
            if (args?.Filter?.ReviewStatus.HasValue == true)
            {
                query.FilterByDecisionType(args.Filter.ReviewStatus.Value, levelId.HasValue == true ? levelId.Value : Guid.Empty, userId);
            }

            if (args?.Filter?.ReviewDecision.HasValue == true)
            {
                query.FilterByPriorDecision(args.Filter.ReviewDecision.Value, this._currentUserService.UserId);
            }

            if (args?.Filter?.CurrentDelenquencyId.HasValue == true)
            {
                query.FilterByDelinquencyId(args.Filter.CurrentDelenquencyId.Value);
            }

            query.SetOrder(args?.Filter?.MoveForward ?? false);

            query.Skip(args?.Offset ?? 0).Take(args?.Limit ?? 50);

            items = await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            count = query.TotalCount ?? 0;

            var properties = this._mapper.Map<IEnumerable<PropertyAssignmentModel>>(items).ToList();
            if (properties.Any())
            {
                var stateRates = await this._stateTaxRatesQuery.ExeсuteAsync(cancellationToken).ConfigureAwait(false);
                foreach (var property in properties)
                {
                    var state = property.Address?.State;
                    if (state != null)
                    {
                        var rate = property.AppraisedValue * stateRates.Where(x => x.State.Id == state.Id).Select(x => x.TaxRate).FirstOrDefault();
                        property.TaxRatio = rate > 0 ? property.AmountDue / rate : 0;
                    }
                }
            }

            return new SearchResultModel<PropertyAssignmentModel>
            {
                TotalCount = count,
                List = properties,
            };
        }

        public async Task<SearchResultModel<DecisionLevelModel>> GetLevelListAsync(SearchArgsModel<DecisionLevelFilterArgs, DecisionLevelSortField> args, CancellationToken cancellationToken = default)
        {
            var query = this._eventDecisionLevelQuery;

            if (args?.Filter?.EventId.HasValue == true)
            {
                query.FilterByEventId(args.Filter.EventId.Value);
            }

            query.Skip(args?.Offset ?? 0).Take(args?.Limit ?? 50);

            var items = this._mapper.Map<IEnumerable<DecisionLevelModel>>(await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false));
            var count = query.TotalCount ?? 0;

            return new SearchResultModel<DecisionLevelModel>
            {
                TotalCount = count,
                List = items,
            };
        }

        public async Task<IEnumerable<ObjectAccessModel>> GetPhotoUrlListAsync(Guid delinquencyId, CancellationToken cancellationToken = default)
        {
            var item = await this._delinquencyQueryProvider.Query
                .Where(x => x.Id == delinquencyId)
                .Select(x => new
                {
                    x.Property.ParcelId,
                    x.Property.County,
                    State = x.Property.State.Abbreviation,
                })
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                throw new NotFoundException();
            }

            var basePath = $"{item.State}/{item.County.Name}/{item.ParcelId}".ToUpper(CultureInfo.CurrentCulture);

            return await this._fileStorage.GetAccessAsync(basePath, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ObjectAccessModel> FindAttachmentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await this._propertyAttachmentQueryProvider.Query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
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
            var item = await this._propertyAttachmentQueryProvider.Query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
            if (item == null)
            {
                throw new NotFoundException();
            }

            if (this._currentUserService.UserId != item.CreatedById)
            {
                throw new NotAcceptableException();
            }

            var command = Command.Create<PropertyAttachmentDeleteCommand>(id, this._currentUserService.UserId);

            await this._serviceBus.PublishAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SearchResultModel<DelinquencyCommentModel>> GetCommentsListAsync(SearchArgsModel<Guid, CommentSortField> args, CancellationToken cancellationToken = default)
        {
            var query = this._delinquencyCommentsQuery.FilterByDelinquencies(new List<Guid> { args.Filter });

            query = query.Skip(args?.Offset ?? 0).Take(args?.Limit ?? 50);

            var items = await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            var count = query.TotalCount ?? 0;

            return new SearchResultModel<DelinquencyCommentModel>
            {
                TotalCount = count,
                List = this._mapper.Map<IEnumerable<DelinquencyCommentModel>>(items),
            };
        }

        public async Task<DelinquencyCommentModel> GetCommentAsync(Guid delinquencyId, Guid commentId, CancellationToken cancellationToken = default)
        {
            return await this._commentQueryProvider.Query
                .Select(x => new DelinquencyCommentModel
                {
                    Id = x.Id,
                    DelinquencyId = x.DelinquencyId,
                    Comment = x.Comment,
                    Author = new FastEntityModel<Guid>
                    {
                        Id = x.Author.Id,
                        Name = x.Author.FirstName + " " + x.Author.LastName,
                    },
                    CommentDate = x.CommentDate,
                    ModifiedOn = x.ModifiedOn,
                })
                .FirstOrDefaultAsync(x => x.DelinquencyId == delinquencyId && x.Id == commentId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<SearchResultModel<DelinquencyHistoryModel>> GetDecisionHistoryAsync(SearchArgsModel<Guid, int> args, CancellationToken cancellationToken = default)
        {
            if (args?.Filter == null || args.Filter == Guid.Empty)
            {
                throw new NotAcceptableException("PropertyId filter parameter should be specified");
            }

            var propertyId = args.Filter;
            var delinquencies = await (from property in this._propertyQueryProvider.Query
                                       join delinquency in this._delinquencyQueryProvider.Query on property.Id equals delinquency.PropertyId
                                       join @event in this._eventQueryProvider.Query on delinquency.EventId equals @event.Id
                                       where property.Id == propertyId && delinquency.DeletedOn == null
                                       select new
                                       {
                                           Date = DateTime.MaxValue,
                                           delinquency.Id,
                                           delinquency.Year,
                                           delinquency.Amount,
                                           delinquency.RUAmount,
                                           delinquency.RULTVPercent,
                                           @event.EventNumber,
                                           @event.SaleDate,
                                       }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var ids = delinquencies.Select(x => x.Id).ToList();
            if (ids.Any() != true)
            {
                return new SearchResultModel<DelinquencyHistoryModel>
                {
                    TotalCount = 0,
                    List = Enumerable.Empty<DelinquencyHistoryModel>(),
                };
            }

            var history = await (from delinquency in this._delinquencyAuditQueryProvider.Query
                                 join @event in this._eventQueryProvider.Query on delinquency.EventId equals @event.Id
                                 where ids.Contains(delinquency.Id)
                                 select new
                                 {
                                     Date = delinquency.InsertedOn,
                                     delinquency.Id,
                                     delinquency.Year,
                                     delinquency.Amount,
                                     delinquency.RUAmount,
                                     delinquency.RULTVPercent,
                                     @event.EventNumber,
                                     @event.SaleDate,
                                 }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var manualDecisions = await (from decision in this._decisionQueryProvider.Query
                                         join user in this._userQueryProvider.Query on decision.UserId equals user.Id
                                         join level in this._eventDecisionLevelQueryProvider.Query on decision.EventDecisionLevelId equals level.Id
                                         where ids.Contains(decision.DelinquencyId)
                                         select new
                                         {
                                             decision.DelinquencyId,
                                             decision.DecisionDate,
                                             DecisionType = (DecisionType?)decision.DecisionTypeId,
                                             DecisionUserId = user.Id,
                                             DecisionUserFirstName = user.FirstName,
                                             DecisionUserLastName = user.LastName,
                                             DecisionComment = decision.Comment,

                                             LevelId = level.Id,
                                             LevelName = level.Name,
                                             LevelOrder = level.Order,
                                             IsFinalLevel = level.IsFinal,
                                         }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var autoDecisions = await this._eventDataCutDecisionQueryProvider.Query
                .Where(x => x.EventDataCutStrategy.IsActive && ids.Contains(x.DelinquencyId))
                .Select(x => new
                {
                    x.DelinquencyId,
                    DecisionDate = x.CreatedOn,
                    DecisionType = x.DecisionTypeId,
                }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var appraisedList = await this._propertyValuationQueryProvider.Query.Where(x => x.PropertyId == propertyId)
                .Select(x => new { Date = DateTime.MaxValue, Value = x.AppraisedValue, Year = x.AppraisedYear, x.ModifiedOn })
                .Union(this._propertyValuationAuditQueryProvider.Query.Where(x => x.PropertyId == propertyId).Select(x => new { Date = x.InsertedOn, Value = x.AppraisedValue, Year = x.AppraisedYear, x.ModifiedOn }))
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var list = new List<DelinquencyHistoryModel>();
            foreach (var delinquency in delinquencies)
            {
                var item = new DelinquencyHistoryModel
                {
                    Id = delinquency.Id,
                    Year = delinquency.Year,
                    EventNumber = delinquency.EventNumber,
                    EventSaleDate = delinquency.SaleDate,
                    Decisions = new List<DecisionHistoryModel>(),
                };

                var autoDecision = autoDecisions.FirstOrDefault(x => x.DelinquencyId == delinquency.Id);
                if (autoDecision != null)
                {
                    var dateTimePoint = autoDecision.DecisionDate;
                    var data = history.Where(x => x.Id == delinquency.Id && x.Date > dateTimePoint).OrderBy(x => x.Date).FirstOrDefault() ?? delinquency;
                    item.Decisions.Add(new DecisionHistoryModel
                    {
                        RUAmount = data.RUAmount ?? 0,
                        RULTV = data.RULTVPercent ?? 0,
                        AssessedValue = (decimal)(appraisedList.Where(x => x.ModifiedOn <= dateTimePoint && x.Date > dateTimePoint).OrderByDescending(x => x.Year).Select(x => x.Value).FirstOrDefault() ?? 0),
                        DecisionDate = autoDecision.DecisionDate,
                        Decision = this._mapper.Map<FastEntityModel<int>>((Enum)(DecisionType)autoDecision.DecisionType),
                    });
                }
                else if (manualDecisions.Any(x => x.DelinquencyId == delinquency.Id) == true)
                {
                    foreach (var manualDecision in manualDecisions.Where(x => x.DelinquencyId == delinquency.Id))
                    {
                        var dateTimePoint = manualDecision.DecisionDate ?? delinquency.SaleDate;
                        var data = history.Where(x => x.Id == delinquency.Id && x.Date > dateTimePoint).OrderBy(x => x.Date).FirstOrDefault() ?? delinquency;
                        item.Decisions.Add(new DecisionHistoryModel
                        {
                            RUAmount = data.RUAmount ?? 0,
                            RULTV = data.RULTVPercent ?? 0,
                            AssessedValue = (decimal)(appraisedList.Where(x => x.ModifiedOn <= dateTimePoint && x.Date > dateTimePoint).OrderByDescending(x => x.Year).Select(x => x.Value).FirstOrDefault() ?? 0),
                            DecisionDate = manualDecision.DecisionDate,
                            Decision = this._mapper.Map<FastEntityModel<int>>((Enum)manualDecision.DecisionType),
                            DecisionComment = manualDecision.DecisionComment,
                            DecisionLevel = manualDecision.LevelName,
                            DecisionLevelOrder = manualDecision.LevelOrder,
                            DecisionUser = new FastEntityModel<Guid>()
                            {
                                Id = manualDecision.DecisionUserId,
                                Name = $"{manualDecision.DecisionUserFirstName} {manualDecision.DecisionUserLastName}",
                            },
                        });
                    }
                }
                else
                {
                    var dateTimePoint = delinquency.SaleDate;
                    var data = history.Where(x => x.Id == delinquency.Id && x.Date > dateTimePoint).OrderBy(x => x.Date).FirstOrDefault() ?? delinquency;
                    item.Decisions.Add(new DecisionHistoryModel
                    {
                        RUAmount = data.RUAmount ?? 0,
                        RULTV = data.RULTVPercent ?? 0,
                        AssessedValue = (decimal)(appraisedList.Where(x => x.ModifiedOn <= dateTimePoint && x.Date > dateTimePoint).OrderByDescending(x => x.Year).Select(x => x.Value).FirstOrDefault() ?? 0),
                        DecisionDate = null,
                        Decision = null,
                    });
                }

                list.Add(item);
            }

            var count = list.Count();

            Func<DelinquencyHistoryModel, object> exp;
            switch (args?.SortField ?? 0)
            {
                case 0:
                    exp = x => x.Year;
                    break;
                case 1:
                    exp = x => x.EventNumber;
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid sort field");
            }

            list = ((args?.SortOrder ?? SortOrder.Asc) == SortOrder.Asc ? list.OrderBy(exp) : list.OrderByDescending(exp))
                .Skip(args?.Offset ?? 0)
                .Take(args?.Limit ?? 50)
                .ToList();

            return new SearchResultModel<DelinquencyHistoryModel>
            {
                TotalCount = count,
                List = list,
            };
        }

        public async Task<PropertyAssignmentModel> GetSnapshotAsync(Guid delinquencyId, DateTime? date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            date = (date ?? currentDate).AddSeconds(30);

            var delinquency = await this._delinquencyQueryProvider.Query
                .Where(x => x.Id == delinquencyId && x.ModifiedOn <= date)
                .Select(x => new
                {
                    Date = currentDate,
                    x.Id,
                    x.CreatedOn,
                    x.ModifiedOn,
                    x.EventId,
                    x.PropertyId,
                    x.Year,
                    x.Amount,
                    x.LTVPercent,
                    x.RUAmount,
                    x.RULTVPercent,
                })
                .Union(this._delinquencyAuditQueryProvider.Query
                    .Where(x => x.Id == delinquencyId && x.ModifiedOn <= date && x.InsertedOn > date)
                    .Select(x => new
                    {
                        Date = x.InsertedOn,
                        x.Id,
                        x.CreatedOn,
                        x.ModifiedOn,
                        x.EventId,
                        x.PropertyId,
                        x.Year,
                        x.Amount,
                        x.LTVPercent,
                        x.RUAmount,
                        x.RULTVPercent,
                    }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (delinquency == null)
            {
                throw new NotFoundException();
            }

            var property = await this._propertyQueryProvider.Query
                .Where(x => x.Id == delinquency.PropertyId && x.ModifiedOn <= date)
                .Select(x => new
                {
                    Date = currentDate,
                    x.Id,
                    x.Address,
                    x.Bankruptcy,
                    x.BuildingSqFt,
                    x.CADId,
                    x.City,
                    x.LandAcres,
                    x.StateId,
                    x.ParcelId,
                    x.LandUseCode,
                    x.LandStateCode,
                    x.LastYearDue,
                    x.GeneralLandUseCodeId,
                    GeneralLandUseCodeName = x.GeneralLandUseCode.Name,
                    x.InternalLandUseCodeId,
                    InternalLandUseCodeName = x.InternalLandUseCode.Name,
                    x.Homestead,
                    x.YearBuilt,
                    x.ZipCode,
                    x.TAXId,
                    x.ImprovementStateCode,
                    x.Latitude,
                    x.Longitude,
                    x.Over65SurvivingSpouse,
                    x.LegalDescription,
                    x.DisabilityExemption,
                    x.Veteran,
                    x.Mortgage,
                    x.PaymentPlan,
                    x.LeadId,
                    x.CountyId,
                    x.ThirdPartyForeclosure,
                })
                .Union(this._propertyAuditQueryProvider.Query
                    .Where(x => x.Id == delinquency.PropertyId && x.ModifiedOn <= date && x.InsertedOn > date)
                    .Select(x => new
                    {
                        Date = x.InsertedOn,
                        x.Id,
                        x.Address,
                        x.Bankruptcy,
                        x.BuildingSqFt,
                        x.CADId,
                        x.City,
                        x.LandAcres,
                        x.StateId,
                        x.ParcelId,
                        x.LandUseCode,
                        x.LandStateCode,
                        x.LastYearDue,
                        x.GeneralLandUseCodeId,
                        GeneralLandUseCodeName = x.GeneralLandUseCode.Name,
                        x.InternalLandUseCodeId,
                        InternalLandUseCodeName = x.InternalLandUseCode.Name,
                        x.Homestead,
                        x.YearBuilt,
                        x.ZipCode,
                        x.TAXId,
                        x.ImprovementStateCode,
                        x.Latitude,
                        x.Longitude,
                        x.Over65SurvivingSpouse,
                        x.LegalDescription,
                        x.DisabilityExemption,
                        x.Veteran,
                        x.Mortgage,
                        x.PaymentPlan,
                        x.LeadId,
                        x.CountyId,
                        x.ThirdPartyForeclosure,
                    }))
                .OrderBy(x => x.Date)
                .FirstAsync(cancellationToken).ConfigureAwait(false);

            if (property == null)
            {
                throw new NotFoundException();
            }

            var eventInfo = await this.GetEventInfoAsync(delinquency.EventId, date.Value, cancellationToken).ConfigureAwait(false);
            var leadInfo = await this.GetLeadInfoAsync(property.LeadId, date.Value, cancellationToken).ConfigureAwait(false);
            var stateInfo = await this.GetStateInfoAsync(property.StateId, date.Value, cancellationToken).ConfigureAwait(false);
            var countyInfo = await this.GetCountyInfoAsync(property.CountyId, date.Value, cancellationToken).ConfigureAwait(false);
            var scoringInfo = await this.GetScoringInfoAsync(delinquencyId, date.Value, cancellationToken).ConfigureAwait(false);
            var displayStrategyInfo = await this.GetDisplayStrategyInfoAsync(delinquencyId, date.Value, cancellationToken).ConfigureAwait(false);
            var decisionsInfo = await this.GetDecisionsInfoAsync(delinquencyId, date.Value, cancellationToken).ConfigureAwait(false);
            var dataCutDecisionInfo = await this.GetDataCutDecisionInfoAsync(delinquencyId, date.Value, cancellationToken).ConfigureAwait(false);
            var (appraisedValue, landValue, improvementValue) = await this.GetPropertyValuationInfoAsync(delinquency.PropertyId, date.Value, cancellationToken).ConfigureAwait(false);
            var suplementalInfo = await this.GetSupplementalInfoAsync(delinquencyId, date.Value, cancellationToken).ConfigureAwait(false);

            var stateRates = await this._stateTaxRatesQuery.FindByStateId(property.StateId).ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            var taxRate = stateRates.Where(x => x.ModifiedOn <= date).OrderByDescending(x => x.ModifiedOn).Select(x => x.TaxRate).FirstOrDefault() * appraisedValue;

            var isLatestEvent = await this._eventQueryProvider.Query
                .Where(x => x.DeletedOn == null && x.Delinquencies.Any(y => y.DeletedOn == null && y.PropertyId == property.Id))
                .Select(x => new { x.Id, x.SaleDate })
                .GroupBy(x => x.SaleDate)
                .OrderByDescending(x => x.Key)
                .Take(1)
                .SelectMany(x => x.Select(y => y.Id))
                .AnyAsync(x => x == eventInfo.Id)
                .ConfigureAwait(false);

            var item = new PropertyAssignmentModel
            {
                Id = delinquencyId,
                CreatedOn = delinquency.CreatedOn,
                ModifiedOn = delinquency.ModifiedOn,
                PropertyId = delinquency.PropertyId,
                DelinquencyYear = delinquency.Year,
                AmountDue = delinquency.Amount,
                RuAmount = delinquency.RUAmount ?? 0,
                Ltv = delinquency.LTVPercent ?? 0,
                RuLtv = delinquency.RULTVPercent ?? 0,
                AppraisedValue = appraisedValue,
                LandValue = landValue,
                ImprovementValue = improvementValue,
                LandAcres = property.LandAcres,
                TaxRatio = taxRate > 0 ? delinquency.Amount / taxRate : 0,
                ParcelId = property.ParcelId,
                LandStateCode = property.LandStateCode,
                LandUseCode = property.LandUseCode,
                IsHomestead = property.Homestead,
                YearBuilt = property.YearBuilt,
                BuildingSqFt = property.BuildingSqFt,
                Bankruptcy = property.Bankruptcy,
                CadId = property.CADId,
                TaxId = property.TAXId,
                LegalDescription = property.LegalDescription,
                DisabilityExemption = property.DisabilityExemption,
                ImprovementStateCode = property.ImprovementStateCode,
                Veteran = property.Veteran,
                Mortgage = property.Mortgage,
                PaymentPlan = property.PaymentPlan,
                Longitude = property.Longitude,
                Latitude = property.Latitude,
                Over65SurvivingSpouse = property.Over65SurvivingSpouse,
                City = property.City,
                ZipCode = property.ZipCode,
                ThirdPartyForeclosure = property.ThirdPartyForeclosure,
                Address = new AddressModel
                {
                    Address1 = property.Address,
                    City = property.City,
                    Zip = property.ZipCode,
                    State = stateInfo,
                },
                GeneralLandUseCode = property.GeneralLandUseCodeId == null ? null : new FastEntityModel<int>
                {
                    Id = property.GeneralLandUseCodeId.Value,
                    Name = property.GeneralLandUseCodeName,
                },
                InternalLandUseCode = property.InternalLandUseCodeId == null ? null : new FastEntityModel<int>
                {
                    Id = property.InternalLandUseCodeId.Value,
                    Name = property.InternalLandUseCodeName,
                },
                Event = eventInfo,
                Lead = leadInfo,
                CountyId = property.CountyId,
                County = countyInfo,
                Supplemental = suplementalInfo,
                Scoring = scoringInfo,
                DisplayStrategy = displayStrategyInfo,
                Decisions = decisionsInfo,
                DataCutDecision = dataCutDecisionInfo,
                IsLatestPropertyData = isLatestEvent,
            };

            return item;
        }

        private async Task<SupplementalModel> GetSupplementalInfoAsync(Guid delinquencyId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._supplementalDataQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.AdvertisementNumber, x.AdvertisementBatch, x.AssessorURL, x.GisURL, x.TreasurerURL, x.OpenLiens, x.ClosedLiens, x.InspectorAreaRating, x.InspectorLawnMaintained, x.InspectorComment, x.InspectorOccupied, x.InspectorPropertyRating, x.InspectorRoofCondition, x.RecentBuyerName, x.RecentBuyerRate, x.LastSaleAmount, x.LastSaleDate, x.MortgageLoanAmount1, x.MortgageLoanAmount2, x.MortgageMaturityDate1, x.MortgageMaturityDate2, x.MortgageOriginationDate1, x.MortgageOriginationDate2, x.MortgageLender1, x.MortgageLender2 })
                .Union(this._supplementalDataAuditQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.AdvertisementNumber, x.AdvertisementBatch, x.AssessorURL, x.GisURL, x.TreasurerURL, x.OpenLiens, x.ClosedLiens, x.InspectorAreaRating, x.InspectorLawnMaintained, x.InspectorComment, x.InspectorOccupied, x.InspectorPropertyRating, x.InspectorRoofCondition, x.RecentBuyerName, x.RecentBuyerRate, x.LastSaleAmount, x.LastSaleDate, x.MortgageLoanAmount1, x.MortgageLoanAmount2, x.MortgageMaturityDate1, x.MortgageMaturityDate2, x.MortgageOriginationDate1, x.MortgageOriginationDate2, x.MortgageLender1, x.MortgageLender2 }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                return null;
            }

            return new SupplementalModel
            {
                OpenLiens = item.OpenLiens,
                ClosedLiens = item.ClosedLiens,
                AssessorURL = item.AssessorURL,
                GisURL = item.GisURL,
                TreasurerURL = item.TreasurerURL,
                AdvertisementNumber = item.AdvertisementNumber,
                AdvertisementBatch = item.AdvertisementBatch,
                InspectorLawnMaintained = item.InspectorLawnMaintained,
                InspectorAreaRating = item.InspectorAreaRating,
                InspectorOccupied = item.InspectorOccupied,
                InspectorPropertyRating = item.InspectorPropertyRating,
                InspectorRoofCondition = item.InspectorRoofCondition,
                InspectorComment = item.InspectorComment,
                RecentBuyerName = item.RecentBuyerName,
                RecentBuyerRate = item.RecentBuyerRate,
                LastSaleAmount = (decimal?)item.LastSaleAmount,
                LastSaleDate = item.LastSaleDate,
                MortgageList = new List<MortgageModel>()
                {
                    new MortgageModel()
                    {
                        MortgageDataNumber = 1,
                        MortgageLender = item.MortgageLender1,
                        MortgageLoanAmount = (decimal?)item.MortgageLoanAmount1,
                        MortgageMaturityDate = item.MortgageMaturityDate1,
                        MortgageOriginationDate = item.MortgageOriginationDate1,
                    },
                    new MortgageModel()
                    {
                        MortgageDataNumber = 2,
                        MortgageLender = item.MortgageLender2,
                        MortgageLoanAmount = (decimal?)item.MortgageLoanAmount2,
                        MortgageMaturityDate = item.MortgageMaturityDate2,
                        MortgageOriginationDate = item.MortgageOriginationDate2,
                    },
                },
            };
        }

        private async Task<int?> GetScoringInfoAsync(Guid delinquencyId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._delinquencyPropertyScoringQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.PropertyScoring })
                .Union(this._delinquencyPropertyScoringAuditQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.PropertyScoring }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            return item?.PropertyScoring;
        }

        private async Task<LeadModel> GetLeadInfoAsync(Guid leadId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._leadQueryProvider.Query.Where(x => x.Id == leadId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.Id, x.AccountName, x.MailingAddress1, x.MailingAddress2, x.MailingAddress3, x.MailingCity, x.MailingZipCode, x.MailingStateId })
                .Union(this._leadAuditQueryProvider.Query.Where(x => x.Id == leadId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.Id, x.AccountName, x.MailingAddress1, x.MailingAddress2, x.MailingAddress3, x.MailingCity, x.MailingZipCode, x.MailingStateId }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                return null;
            }

            FastEntityModel<int> stateInfo = null;
            if (item.MailingStateId.HasValue)
            {
                stateInfo = await this.GetStateInfoAsync(item.MailingStateId.Value, date, cancellationToken).ConfigureAwait(false);
            }

            return new LeadModel
            {
                Id = item.Id,
                Name = item.AccountName,
                Address = new AddressModel
                {
                    Address1 = item.MailingAddress1,
                    Address2 = item.MailingAddress2,
                    Address3 = item.MailingAddress3,
                    City = item.MailingCity,
                    Zip = item.MailingZipCode,
                    State = stateInfo,
                },
            };
        }

        private async Task<FastEntityModel<int>> GetCountyInfoAsync(int countyId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._countyQueryProvider.Query.Where(x => x.Id == countyId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.Id, x.Name })
                .Union(this._countyAuditQueryProvider.Query.Where(x => x.Id == countyId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.Id, x.Name }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                return null;
            }

            return new FastEntityModel<int>
            {
                Id = item.Id,
                Name = item.Name,
            };
        }

        private async Task<FastEntityModel<int>> GetStateInfoAsync(int stateId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._stateQueryProvider.Query.Where(x => x.Id == stateId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.Id, x.Abbreviation })
                .Union(this._stateAuditQueryProvider.Query.Where(x => x.Id == stateId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.Id, x.Abbreviation }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                return null;
            }

            return new FastEntityModel<int>
            {
                Id = item.Id,
                Name = item.Abbreviation,
            };
        }

        private async Task<(decimal AppraisedValue, decimal LandValue, decimal ImprovementValue)> GetPropertyValuationInfoAsync(Guid propertyId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._propertyValuationQueryProvider.Query.Where(x => x.PropertyId == propertyId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.AppraisedValue, x.LandValue, x.ImprovementValue, x.AppraisedYear })
                .Union(this._propertyValuationAuditQueryProvider.Query.Where(x => x.PropertyId == propertyId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.AppraisedValue, x.LandValue, x.ImprovementValue, x.AppraisedYear }))
                .OrderByDescending(x => x.AppraisedYear)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                return default;
            }

            return ((decimal)(item.AppraisedValue ?? 0), (decimal)(item.LandValue ?? 0), (decimal)(item.ImprovementValue ?? 0));
        }

        private async Task<IEnumerable<int>> GetDisplayStrategyInfoAsync(Guid delinquencyId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var items = await this._delinquencyPropertyDisplayStrategyQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date)
                .Select(x => x.PropertyDisplayStrategyId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var historyList = await this._delinquencyPropertyDisplayStrategyAuditQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date && x.InsertedOn > date)
                .GroupBy(x => x.OperationId)
                .Select(x => new
                {
                    Date = x.First().InsertedOn,
                    Values = x.Select(y => y.PropertyDisplayStrategyId),
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var item = historyList.Union(new[] { new { Date = currentDate, Values = items.AsEnumerable() } }).OrderBy(x => x.Date).FirstOrDefault();
            return item?.Values ?? Enumerable.Empty<int>();
        }

        private async Task<EventModel> GetEventInfoAsync(Guid eventId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._eventQueryProvider.Query.Where(x => x.Id == eventId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.Id, x.EventNumber, x.IsLocked, x.SaleDate, x.FundingDate, x.DueDate, x.IsRejectReasonRequired, x.CountyId, x.StateId, x.EventTypeId, x.AuctionTypeId })
                .Union(this._eventAuditQueryProvider.Query.Where(x => x.Id == eventId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.Id, x.EventNumber, x.IsLocked, x.SaleDate, x.FundingDate, x.DueDate, x.IsRejectReasonRequired, x.CountyId, x.StateId, x.EventTypeId, x.AuctionTypeId }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                return null;
            }

            var eventType = await this._eventTypeQueryProvider.Query.Where(x => x.Id == item.EventTypeId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.Id, x.Name, x.Description })
                .Union(this._eventTypeAuditQueryProvider.Query.Where(x => x.Id == item.EventTypeId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.Id, x.Name, x.Description }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            var auctionType = await this._auctionTypeQueryProvider.Query.Where(x => x.Id == item.AuctionTypeId && x.ModifiedOn <= date)
                .Select(x => new { Date = currentDate, x.Id, x.Name, x.Description })
                .Union(this._auctionTypeAuditQueryProvider.Query.Where(x => x.Id == item.AuctionTypeId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, x.Id, x.Name, x.Description }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            var isAssigned = await this._eventDecisionLevelQueryProvider.Query
                .Where(x => x.EventId == eventId && x.CreatedOn <= date && (x.DeletedOn == null || x.DeletedOn > date))
                .AnyAsync(cancellationToken).ConfigureAwait(false);

            var state = await this.GetStateInfoAsync(item.StateId, date, cancellationToken).ConfigureAwait(false);
            var county = await this.GetCountyInfoAsync(item.CountyId, date, cancellationToken).ConfigureAwait(false);

            return new EventModel
            {
                Id = item.Id,
                Number = item.EventNumber,
                SaleDate = item.SaleDate,
                FundingDate = item.FundingDate,
                DueDate = item.DueDate,
                IsLocked = item.IsLocked,
                IsRejectReasonRequired = item.IsRejectReasonRequired,
                Type = eventType == null ? null : new FastEntityModel<int> { Id = eventType.Id, Name = eventType.Name },
                AuctionType = auctionType == null ? null : new FastEntityModel<int> { Id = auctionType.Id, Name = auctionType.Name },
                State = state,
                County = county,
                IsAssigned = isAssigned,
                AssignedTo = null,
            };
        }

        private async Task<IEnumerable<DecisionModel>> GetDecisionsInfoAsync(Guid delinquencyId, DateTime date, CancellationToken cancellationToken = default)
        {
            var items = await this._decisionQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date)
                .Select(x => new
                {
                    x.Id,
                    x.DecisionDate,
                    DecisionType = (DecisionType?)x.DecisionTypeId,
                    x.Comment,
                    x.UserId,
                    LevelId = x.EventDecisionLevelId,
                })
                .Union(this._decisionAuditQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date && x.InsertedOn > date)
                .Select(y => new
                {
                    y.Id,
                    y.DecisionDate,
                    DecisionType = (DecisionType?)y.DecisionTypeId,
                    y.Comment,
                    y.UserId,
                    LevelId = y.EventDecisionLevelId,
                })).ToListAsync(cancellationToken).ConfigureAwait(false);

            if (items.Any() == false)
            {
                return Enumerable.Empty<DecisionModel>();
            }

            var levelIds = items.Select(x => x.LevelId).Distinct();
            var levels = await this._eventDecisionLevelQueryProvider.Query.Where(x => levelIds.Contains(x.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);

            return items.Select(x => new DecisionModel
            {
                Id = x.Id,
                Type = this._mapper.Map<FastEntityModel<int>>((Enum)x.DecisionType),
                DecisionDate = x.DecisionDate,
                Comment = x.Comment,
                UserId = x.UserId,
                Level = new DecisionLevelModel
                {
                    Id = x.LevelId,
                    Name = levels.First(y => y.Id == x.LevelId).Name,
                    Order = levels.First(y => y.Id == x.LevelId).Order,
                    IsFinal = levels.First(y => y.Id == x.LevelId).IsFinal,
                },
            });
        }

        private async Task<FastEntityModel<int>> GetDataCutDecisionInfoAsync(Guid delinquencyId, DateTime date, CancellationToken cancellationToken = default)
        {
            var currentDate = this._clockService.UtcNow;
            var item = await this._eventDataCutDecisionQueryProvider.Query.Where(x => x.DelinquencyId == delinquencyId && x.ModifiedOn <= date && x.EventDataCutStrategy.IsActive)
                .Select(x => new { Date = currentDate, DecisionType = (DecisionType)x.DecisionTypeId })
                .Union(this._eventDataCutDecisionAuditQueryProvider.Query.Where(x => x.Id == delinquencyId && x.ModifiedOn <= date && x.InsertedOn > date).Select(x => new { Date = x.InsertedOn, DecisionType = (DecisionType)x.DecisionTypeId }))
                .OrderBy(x => x.Date)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                return null;
            }

            return this._mapper.Map<FastEntityModel<int>>((Enum)item.DecisionType);
        }
    }
}
