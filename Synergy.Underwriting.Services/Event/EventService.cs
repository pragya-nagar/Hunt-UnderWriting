using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Synergy.Common.Exceptions;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Messages;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Attachment;

namespace Synergy.Underwriting.Services
{
    public class EventService :
        IMessageHandler<EventCreateCommand>,
        IMessageHandler<EventUpdateCommand>,
        IMessageHandler<AttachmentCreateCommand>,
        IMessageHandler<AssignmentLevelCreateCommand>,
        IMessageHandler<AssignmentLevelUpdateCommand>,
        IMessageHandler<RulesToEventAttachCommand>,
        IMessageHandler<RulesToEventUpdateCommand>,
        IMessageHandler<SetEventLockStatusCommand>,
        IMessageHandler<EventAttachmentDeleteCommand>
    {
        private readonly Guid _systemUserId = Common.Constants.User.SystemUserId;

        private readonly ILogger _logger;
        private readonly IPublishMessage _serviceBus;
        private readonly IMapper _mapper;

        private readonly ICreateEventCommand _createEventCommand;
        private readonly IUpdateEventCommand _updateEventCommand;
        private readonly IAttachFileToEventCommand _attachFileToEventCommand;
        private readonly ICreateEventDecisionLevelCommand _createEventDecisionLevelCommand;
        private readonly IAssignUserToReviewDelinquencyCommand _assignUserToReviewDelinquencyCommand;
        private readonly IReassignUserToReviewDelinquencyCommand _reassignUserToReviewDelinquencyCommand;
        private readonly IAttachRulesToEventCommand _attachRulesToEventCommand;
        private readonly IAddRulesToEventCommand _addRulesToEventCommand;
        private readonly ICreateEventDataCutDecisionsCommand _createDataCutCommand;
        private readonly ISetEventLockStatusCommand _setEventLockStatusCommand;
        private readonly IDeleteEventAttachmentCommand _deleteEventAttachmentCommand;
        private readonly IBulkCreatePropertyProfileDelinquencyCommand _bulkCreatePropertyProfileDelinquencyCommand;
        private readonly IRemovePropertyProfileDelinquencyCommand _removePropertyProfileDelinquencyCommand;

        private readonly CheckEventExistsQuery _checkEventExistsQuery;
        private readonly EventDelinquencyIdsQuery _eventDelinquencyIdsQuery;
        private readonly EventDataCutDelinquencyIdsQuery _eventDataCutDelinquencyIdsQuery;
        private readonly EventDecisionLevelsQuery _eventDecisionLevelsQuery;
        private readonly EventDecisionQuery _eventDecisionQuery;
        private readonly UnderwritingWorkflowDefinitionQuery _underwritingWorkflowDefinitionQuery;
        private readonly GetPropertyProfileByEventIdQuery _getPropertyProfileByEventIdQuery;
        private readonly GetCountyNameQuery _countyNameQuery;
        private readonly GetStateAbbreviationQuery _stateAbbreviationQuery;
        private readonly GetEventTypeQuery _eventTypeQuery;
        private readonly GetEventNamesByLocationQuery _eventNamesByLocationQuery;
        private readonly GetEventLockStatusQuery _eventLockStatusQuery;
        private readonly GetEventQuery _eventQuery;
        private readonly EventsAssignmentsMetadataQuery _eventsAssignmentsMetadataQuery;

        public EventService(ILogger<EventService> logger,
            IPublishMessage serviceBus,
            IMapper mapper,
            ICreateEventCommand createEventCommand,
            IUpdateEventCommand updateEventCommand,
            IAttachFileToEventCommand attachFileToEventCommand,
            ICreateEventDecisionLevelCommand createEventDecisionLevelCommand,
            IAssignUserToReviewDelinquencyCommand assignUserToReviewDelinquencyCommand,
            IReassignUserToReviewDelinquencyCommand reassignUserToReviewDelinquencyCommand,
            IAttachRulesToEventCommand attachRulesToEventCommand,
            IAddRulesToEventCommand addRulesToEventCommand,
            ICreateEventDataCutDecisionsCommand createDataCutCommand,
            IDeleteEventAttachmentCommand deleteEventAttachmentCommand,
            ISetEventLockStatusCommand setEventLockStatusCommand,
            IBulkCreatePropertyProfileDelinquencyCommand bulkCreatePropertyProfileDelinquencyCommand,
            IRemovePropertyProfileDelinquencyCommand removePropertyProfileDelinquencyCommand,
            CheckEventExistsQuery checkEventExistsQuery,
            UnderwritingWorkflowDefinitionQuery underwritingWorkflowDefinitionQuery,
            EventDelinquencyIdsQuery eventDelinquencyIdsQuery,
            EventDataCutDelinquencyIdsQuery eventDataCutDelinquencyIdsQuery,
            EventDecisionLevelsQuery eventDecisionLevelsQuery,
            EventDecisionQuery eventDecisionQuery,
            GetPropertyProfileByEventIdQuery getPropertyProfileByEventIdQuery,
            GetCountyNameQuery countyNameQuery,
            GetStateAbbreviationQuery stateAbbreviationQuery,
            GetEventTypeQuery eventTypeQuery,
            GetEventNamesByLocationQuery eventNamesByLocationQuery,
            GetEventLockStatusQuery eventLockStatusQuery,
            GetEventQuery eventQuery,
            EventsAssignmentsMetadataQuery eventsAssignmentsMetadataQuery)
        {
            this._logger = logger;
            this._serviceBus = serviceBus;
            this._mapper = mapper;
            this._createEventCommand = createEventCommand;
            this._updateEventCommand = updateEventCommand;
            this._attachFileToEventCommand = attachFileToEventCommand;
            this._createEventDecisionLevelCommand = createEventDecisionLevelCommand;
            this._assignUserToReviewDelinquencyCommand = assignUserToReviewDelinquencyCommand;
            this._reassignUserToReviewDelinquencyCommand = reassignUserToReviewDelinquencyCommand;
            this._attachRulesToEventCommand = attachRulesToEventCommand;
            this._addRulesToEventCommand = addRulesToEventCommand;
            this._createDataCutCommand = createDataCutCommand;
            this._setEventLockStatusCommand = setEventLockStatusCommand;
            this._deleteEventAttachmentCommand = deleteEventAttachmentCommand;
            this._bulkCreatePropertyProfileDelinquencyCommand = bulkCreatePropertyProfileDelinquencyCommand;
            this._removePropertyProfileDelinquencyCommand = removePropertyProfileDelinquencyCommand;

            this._checkEventExistsQuery = checkEventExistsQuery;
            this._eventDelinquencyIdsQuery = eventDelinquencyIdsQuery;
            this._eventDataCutDelinquencyIdsQuery = eventDataCutDelinquencyIdsQuery;
            this._eventDecisionLevelsQuery = eventDecisionLevelsQuery;
            this._eventDecisionQuery = eventDecisionQuery;
            this._getPropertyProfileByEventIdQuery = getPropertyProfileByEventIdQuery;
            this._countyNameQuery = countyNameQuery;
            this._stateAbbreviationQuery = stateAbbreviationQuery;
            this._eventTypeQuery = eventTypeQuery;
            this._eventNamesByLocationQuery = eventNamesByLocationQuery;
            this._eventLockStatusQuery = eventLockStatusQuery;
            this._eventQuery = eventQuery ?? throw new ArgumentNullException(nameof(eventQuery));
            this._underwritingWorkflowDefinitionQuery = underwritingWorkflowDefinitionQuery ?? throw new ArgumentNullException(nameof(underwritingWorkflowDefinitionQuery));
            this._eventsAssignmentsMetadataQuery = eventsAssignmentsMetadataQuery ?? throw new ArgumentNullException(nameof(eventsAssignmentsMetadataQuery));
        }

        public void Handle(EventCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(EventCreateCommand message, CancellationToken cancellationToken = default)
        {
            var cmd = this._mapper.Map<CreateEventModel>(message);

            cmd.EventNumber = await this.GenerateEventNumberAsync(cmd, cancellationToken).ConfigureAwait(false);

            var workflowDefinitionId = await this._underwritingWorkflowDefinitionQuery.ExecuteAsync((cmd.StateId, cmd.EventTypeId), cancellationToken).ConfigureAwait(false);
            if (workflowDefinitionId == null)
            {
                throw new NotFoundException($"Please note, there is no Workflow configured for \"State\"/\"Event type\". Event '{cmd.EventNumber}' couldn’t be created, please configure proper workflow template first.");
            }

            await this._createEventCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            var evt = ServiceBus.Abstracts.Event.Create<EventCreatedEvent>(message.Id, message.CreatedBy);

            evt.Number = cmd.EventNumber;
            evt.TypeId = cmd.EventTypeId;
            evt.StateId = cmd.StateId;
            evt.SaleDate = cmd.SaleDate;
            evt.FundingDate = cmd.FundingDate;
            evt.DepartmentUserIds = cmd.UserDepartments.GroupBy(x => x.DepartmentId).ToDictionary(x => x.Key, x => x.Select(y => y.UserId));

            await this._serviceBus.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(EventUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(EventUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var e = await this._eventQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);
            if (e == null)
            {
                throw new NotFoundException($"Event '{message.Id}' does not exist");
            }

            var cmd = this._mapper.Map<UpdateEventModel>(message);

            if (e.StateId == cmd.StateId
                && cmd.CountyId.HasValue == true
                && e.CountyId == cmd.CountyId
                && e.EventTypeId == cmd.EventTypeId
                && e.SaleDate.Year == cmd.SaleDate.Year)
            {
                cmd.EventNumber = e.EventNumber;
            }
            else
            {
                cmd.EventNumber = await this.GenerateEventNumberAsync(cmd, cancellationToken).ConfigureAwait(false);
            }

            await this._updateEventCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            var eventMetadata = await this._eventsAssignmentsMetadataQuery.ExecuteAsync(new[] { message.Id }, cancellationToken).ConfigureAwait(false);
            var manualDelinquencyCount = eventMetadata.Where(x => x.Id == message.Id).Select(x => x.ManualDelinquencyCount).FirstOrDefault();

            var evt = ServiceBus.Abstracts.Event.Create<EventUpdatedEvent>(message.Id, message.CreatedBy);

            evt.Number = cmd.EventNumber;
            evt.TypeId = cmd.EventTypeId;
            evt.StateId = cmd.StateId;
            evt.SaleDate = cmd.SaleDate;
            evt.FundingDate = cmd.FundingDate;
            evt.DepartmentUserIds = cmd.UserDepartments.GroupBy(x => x.DepartmentId).ToDictionary(x => x.Key, x => x.Select(y => y.UserId));
            evt.ManualDelinquencyCount = manualDelinquencyCount;

            await this._serviceBus.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(AttachmentCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(AttachmentCreateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not exist");
            }

            var cmd = this._mapper.Map<AttachFileToEventModel>(message);
            await this._attachFileToEventCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(AssignmentLevelCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(AssignmentLevelCreateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not exist");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var decisionLevels = await this._eventDecisionLevelsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            foreach (var assigment in message.Assigments)
            {
                if (decisionLevels.Any(x => x.Order == assigment.Order) == true)
                {
                    throw new NotAcceptableException("Level with the same order already exists");
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (assigment.IsFinal && decisionLevels.Any(x => x.IsFinal) == true)
                {
                    throw new NotAcceptableException("The final level already exist. The final level should be unique for an event");
                }
            }

            var delinquencyIds = await this._eventDelinquencyIdsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (delinquencyIds.Any() == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not have delinquency for assignment");
            }

            cancellationToken.ThrowIfCancellationRequested();
            this._logger.LogInformation("{DelinquencyCount} delinquencies have been discovered", delinquencyIds.Count());

            var dataCutDelinquencyIds = await this._eventDataCutDelinquencyIdsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            this._logger.LogInformation("{DelinquencyCount} delinquencies have been processed by data cut", dataCutDelinquencyIds.Count());

            var manualDelinquencyIds = delinquencyIds.Except(dataCutDelinquencyIds).OrderBy(x => x).ToList();
            if (manualDelinquencyIds.Any() == false)
            {
                throw new ModelStateException($"Event '{message.EventId}' does not have delinquency for assignment. All delinquency have been processed by data cut");
            }

            foreach (var assigment in message.Assigments)
            {
                var amountToAssign = assigment.Assignments.Values.Sum();
                if (amountToAssign != manualDelinquencyIds.Count)
                {
                    throw new NotAcceptableException($"{manualDelinquencyIds.Count} delinquencies have been discovered for manual decision. Assignments amount {amountToAssign} is not correct.");
                }
            }

            this._logger.LogInformation("{DelinquencyCount} delinquencies have been discovered for manual decision", manualDelinquencyIds.Count);

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(20),
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var assigment in message.Assigments)
                {
                    var level = new CreateEventDecisionLevelModel
                    {
                        Id = assigment.LevelId,
                        EventId = message.EventId,
                        Name = assigment.Name,
                        IsFinal = assigment.IsFinal,
                        Order = assigment.Order,
                    };

                    await this._createEventDecisionLevelCommand.DispatchAsync(level, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                    var assignments = dataCutDelinquencyIds.Select(delinquencyId => new AssignUserToReviewDelinquencyModel
                    {
                        Id = Guid.NewGuid(),
                        EventDecisionLevelId = assigment.LevelId,
                        DelinquencyId = delinquencyId,
                        UserId = this._systemUserId,
                    }).ToList();

                    var index = 0;

                    foreach (var userId in assigment.Assignments.Keys)
                    {
                        var quantity = assigment.Assignments[userId];

                        if (manualDelinquencyIds.Count < index + quantity)
                        {
                            throw new NotAcceptableException("Invalid assignment configuration.");
                        }

                        assignments.AddRange(manualDelinquencyIds.GetRange(index, quantity).Select(delinquencyId => new AssignUserToReviewDelinquencyModel
                        {
                            Id = Guid.NewGuid(),
                            EventDecisionLevelId = assigment.LevelId,
                            DelinquencyId = delinquencyId,
                            UserId = userId,
                        }));

                        index += quantity;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    await this._assignUserToReviewDelinquencyCommand.DispatchAsync(assignments, message.CreatedBy, cancellationToken).ConfigureAwait(false);
                }

                scope.Complete();

                var eventMetadataList = await this._eventsAssignmentsMetadataQuery.ExecuteAsync(new[] { message.EventId }, cancellationToken).ConfigureAwait(false);
                var metadata = eventMetadataList.First();

                var evt = ServiceBus.Abstracts.Event.Create<EventAssignedEvent>(message.EventId, message.CreatedBy);

                evt.NLevelUsers = metadata.NLevelUsers;
                evt.FinalLevelUsers = metadata.FinalLevelUsers;

                await this._serviceBus.PublishAsync(evt, cancellationToken).ConfigureAwait(false);

                this._logger.LogInformation("An assignment have finished for the event '{EventId}'", message.EventId);
            }
        }

        public void Handle(AssignmentLevelUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(AssignmentLevelUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not exist");
            }

            var decisions = await this._eventDecisionQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (decisions.Any() == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not have delinquency for processing");
            }

            var dataCutDelinquencyIds = await this._eventDataCutDelinquencyIdsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("{DelinquencyCount} delinquencies have been processed by data cut", dataCutDelinquencyIds.Count());

            var emptyDecisions = decisions
                     .Where(x => x.DecisionType == null)
                     .GroupJoin(dataCutDelinquencyIds, x => x.DelinquencyId, x => x, (d, dc) => new { d, dc })
                     .Where(x => x.dc.Any() == false)
                     .Select(x => x.d)
                     .GroupBy(x => x.LevelId);

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var levelId in message.Assigments.Select(x => x.LevelId))
            {
                var d = emptyDecisions.FirstOrDefault(x => x.Key == levelId);

                if (d == null || d.Any() == false)
                {
                    throw new NotAcceptableException($"Level '{levelId}' does not have delinquency for processing. All {d.Count()} delinquencies have decision.");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            this._logger.LogInformation("Empty desitions found.");

            var reassignLevels = new List<ReassignUsersModel>();

            foreach (var messageAssigment in message.Assigments)
            {
                var reassign = new ReassignUsersModel
                {
                    LevelId = messageAssigment.LevelId,
                    Assignments = new List<(Guid userId, Guid decisionId)>(),
                };

                var emptyLevelDecisions = emptyDecisions.FirstOrDefault(x => x.Key == messageAssigment.LevelId);

                var index = 0;

                foreach (var userId in messageAssigment.Assignments.Keys)
                {
                    var quantity = messageAssigment.Assignments[userId];

                    if (emptyLevelDecisions.Count() < index + quantity)
                    {
                        throw new NotAcceptableException("Invalid assignment configuration.");
                    }

                    reassign.Assignments.AddRange(emptyLevelDecisions.Skip(index).Take(quantity).Select(decision => (userId, decisionId: decision.Id)));

                    index += quantity;
                }

                reassignLevels.Add(reassign);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await this._reassignUserToReviewDelinquencyCommand.DispatchAsync(reassignLevels, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            var eventMetadataList = await this._eventsAssignmentsMetadataQuery.ExecuteAsync(new[] { message.EventId }, cancellationToken).ConfigureAwait(false);
            var metadata = eventMetadataList.First();

            var evt = ServiceBus.Abstracts.Event.Create<EventAssignedEvent>(message.EventId, message.CreatedBy);

            evt.NLevelUsers = metadata.NLevelUsers;
            evt.FinalLevelUsers = metadata.FinalLevelUsers;

            await this._serviceBus.PublishAsync(evt, cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("An assignment have finished for the event '{EventId}'", message.EventId);
        }

        public void Handle(RulesToEventAttachCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(RulesToEventAttachCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not exist");
            }

            var cmd = this._mapper.Map<AttachRulesToEventModel>(message);
            await this._attachRulesToEventCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(RulesToEventUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(RulesToEventUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not exist");
            }

            var cmd = this._mapper.Map<AddRulesToEventModel>(message);
            await this._addRulesToEventCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(SetEventLockStatusCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(SetEventLockStatusCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not exist");
            }

            await this._setEventLockStatusCommand.DispatchAsync(message.EventId, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            var isLocked = await this._eventLockStatusQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (isLocked == true)
            {
                var evt = ServiceBus.Abstracts.Event.Create<EventLockedEvent>(message.EventId, message.CreatedBy);
                await this._serviceBus.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var evt = ServiceBus.Abstracts.Event.Create<EventUnlockedEvent>(message.EventId, message.CreatedBy);
                await this._serviceBus.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Handle(EventAttachmentDeleteCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(EventAttachmentDeleteCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.Id}' does not exist");
            }

            var cmd = this._mapper.Map<DeleteAttachmentModel>(message);
            await this._deleteEventAttachmentCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> GenerateEventNumberAsync(UpdateEventModel eventCreate, CancellationToken cancellationToken)
        {
            string countyName;
            if (eventCreate.CountyId.HasValue)
            {
                countyName = await this._countyNameQuery.ExecuteAsync(eventCreate.CountyId.Value, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                countyName = eventCreate.CountyName.Trim();
            }

            var year = eventCreate.SaleDate.ToString("yy", CultureInfo.CurrentCulture);

            var stateAbbreviation = await this._stateAbbreviationQuery.ExecuteAsync(eventCreate.StateId, cancellationToken).ConfigureAwait(false);

            var eventType = await this._eventTypeQuery.ExecuteAsync(eventCreate.EventTypeId, cancellationToken).ConfigureAwait(false);

            var names = await this._eventNamesByLocationQuery.ExecuteAsync((eventCreate.StateId, countyName, eventCreate.EventTypeId, eventCreate.SaleDate.Year), cancellationToken).ConfigureAwait(false);

            var namePrefix = $"{stateAbbreviation.Trim()}{year}-{countyName.Trim()}-{eventType.ToUpperInvariant().Trim()}-B";

            var sequenceNumbers = names.Select(x => x.Length > namePrefix.Length && int.TryParse(x.Substring(namePrefix.Length), out var number) ? number : 0);

            var nextNumber = sequenceNumbers.DefaultIfEmpty().Max(x => x) + 1;

            return namePrefix + nextNumber;
        }
    }
}
