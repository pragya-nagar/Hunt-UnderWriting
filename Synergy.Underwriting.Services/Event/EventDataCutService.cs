using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Synergy.Common.Exceptions;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Messages;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services.Event
{
    public class EventDataCutService : IMessageHandler<RuleCreateCommand>, IMessageHandler<ApplyRulesCommand>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPublishMessage _publisher;

        private readonly GetCountyByEvent _getCountyQuery;
        private readonly ICreateSingleDataCutRuleCommand _createSingleDataCutRuleCommand;
        private readonly ICreateDataCutRuleCommand _createDataCutRuleCommand;
        private readonly IAddRulesToEventCommand _addRulesToEventCommand;
        private readonly DataCutQuery _dataCutQuery;
        private readonly ICreateEventDataCutDecisionsCommand _createEventDataCutDecisionsCommand;
        private readonly EventsAssignmentsMetadataQuery _eventsAssignmentsMetadataQuery;

        public EventDataCutService(ILogger<EventService> logger,
            IMapper mapper,
            IPublishMessage publisher,
            GetCountyByEvent getCountyQuery,
            ICreateSingleDataCutRuleCommand createSingleDataCutRuleCommand,
            ICreateDataCutRuleCommand createDataCutRuleCommand,
            IAddRulesToEventCommand addRulesToEventCommand,
            DataCutQuery dataCutQuery,
            ICreateEventDataCutDecisionsCommand createEventDataCutDecisionsCommand,
            EventsAssignmentsMetadataQuery eventsAssignmentsMetadataQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._getCountyQuery = getCountyQuery ?? throw new ArgumentNullException(nameof(getCountyQuery));
            this._createSingleDataCutRuleCommand = createSingleDataCutRuleCommand ?? throw new ArgumentNullException(nameof(createSingleDataCutRuleCommand));
            this._createDataCutRuleCommand = createDataCutRuleCommand ?? throw new ArgumentNullException(nameof(createDataCutRuleCommand));
            this._addRulesToEventCommand = addRulesToEventCommand ?? throw new ArgumentNullException(nameof(createDataCutRuleCommand));
            this._dataCutQuery = dataCutQuery ?? throw new ArgumentNullException(nameof(dataCutQuery));
            this._createEventDataCutDecisionsCommand = createEventDataCutDecisionsCommand ?? throw new ArgumentNullException(nameof(createEventDataCutDecisionsCommand));
            this._eventsAssignmentsMetadataQuery = eventsAssignmentsMetadataQuery ?? throw new ArgumentNullException(nameof(eventsAssignmentsMetadataQuery));
        }

        public void Handle(RuleCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(RuleCreateCommand message, CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var entity = this._mapper.Map<CreateDataCutRuleModel>(message);
            await this._createSingleDataCutRuleCommand.DispatchAsync(entity, message.CreatedBy, cancellationToken).ConfigureAwait(false);
            this._logger.LogInformation("new rule has been added");
        }

        public void Handle(ApplyRulesCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(ApplyRulesCommand message, CancellationToken cancellationToken = default)
        {
            var eventId = message.EventId;
            var countyId = await this._getCountyQuery.ExecuteAsync(eventId, cancellationToken).ConfigureAwait(false);
            if (countyId == 0)
            {
                throw new NotFoundException($"Event with id '{eventId}' does not exist");
            }

            var ruleIds = message.RuleIds.ToList();
            cancellationToken.ThrowIfCancellationRequested();

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(20),
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                await this._addRulesToEventCommand.DispatchAsync(new AddRulesToEventModel
                {
                    EventId = eventId,
                    DataCutRuleIds = ruleIds,
                }, message.CreatedBy,
                   cancellationToken).ConfigureAwait(false);

                this._logger.LogInformation("{RulesCount} rules have been assigned to the event '{EventId}'", ruleIds.Count(), eventId);

                var eventDataCutDecisionModel = await this._dataCutQuery.ExecuteAsync(eventId, cancellationToken).ConfigureAwait(false);

                if (eventDataCutDecisionModel.Any() == true)
                {
                    await this._createEventDataCutDecisionsCommand.DispatchAsync(eventDataCutDecisionModel, message.CreatedBy, cancellationToken).ConfigureAwait(false);
                }

                scope.Complete();
            }

            this._logger.LogInformation("An apply rules have finished for the event '{EventId}'", eventId);

            var eventMetadataList = await this._eventsAssignmentsMetadataQuery.ExecuteAsync(new[] { eventId }, cancellationToken).ConfigureAwait(false);
            var metadata = eventMetadataList.First();

            var @event = ServiceBus.Abstracts.Event.Create<DataCutProcessedEvent>(eventId, message.CreatedBy);

            @event.ManualDelinquencyCount = metadata.ManualDelinquencyCount;
            @event.NLevelUsers = metadata.NLevelUsers;
            @event.FinalLevelUsers = metadata.FinalLevelUsers;

            await this._publisher.PublishAsync(@event, cancellationToken).ConfigureAwait(false);
        }
    }
}
