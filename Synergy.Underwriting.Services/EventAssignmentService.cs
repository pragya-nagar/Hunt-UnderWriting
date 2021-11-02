using System;
using System.Collections.Generic;
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
using Synergy.Underwriting.DAL.Commands.Models.ProfileAssignment;
using Synergy.Underwriting.DAL.Commands.Models.Results;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands.Event;
using Synergy.Underwriting.Models.Commands.EventAssignment;

namespace Synergy.Underwriting.Services
{
    public class EventAssignmentService : IMessageHandler<EventAssignmentCreateCommand>,
                                          IMessageHandler<EventAssignmentUpdateCommand>,
                                          IMessageHandler<EventAssignmentPerformCommand>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPublishMessage _publisher;
        private readonly ICreateAssignmentConfigurationCommand _createAssignmentConfigurationCommand;
        private readonly IUpdateAssignmentConfigurationCommand _updateAssignmentConfigurationCommand;
        private readonly IDeleteEventDecisionCommand _deleteEventDecisionCommand;
        private readonly ICreateProfileAssignmentCommand _createProfileAssignmentCommand;
        private readonly ICreateOtherAssignmentCommand _createOtherAssignmentCommand;

        private readonly CheckEventExistsQuery _eventExistsQuery;
        private readonly EventDelinquencyIdsQuery _eventDelinquencyIdsQuery;
        private readonly EventDataCutDelinquencyIdsQuery _eventDataCutDelinquencyIdsQuery;
        private readonly EventDecisionLevelsQuery _eventDecisionLevelsQuery;
        private readonly DecisionLevelProfileQuery _decisionLevelProfileQuery;
        private readonly EventsAssignmentsMetadataQuery _eventsAssignmentsMetadataQuery;

        public EventAssignmentService(IMapper mapper,
                               ILogger<EventAssignmentService> logger,
                               IPublishMessage publisher,
                               ICreateAssignmentConfigurationCommand createAssignmentConfigurationCommand,
                               IUpdateAssignmentConfigurationCommand updateAssignmentConfigurationCommand,
                               IDeleteEventDecisionCommand deleteEventDecisionCommand,
                               ICreateProfileAssignmentCommand createProfileAssignmentCommand,
                               ICreateOtherAssignmentCommand createOtherAssignmentCommand,
                               CheckEventExistsQuery eventExistsQuery,
                               EventDelinquencyIdsQuery eventDelinquencyIdsQuery,
                               EventDataCutDelinquencyIdsQuery eventDataCutDelinquencyIdsQuery,
                               EventDecisionLevelsQuery eventDecisionLevelsQuery,
                               DecisionLevelProfileQuery decisionLevelProfileQuery,
                               EventsAssignmentsMetadataQuery eventsAssignmentsMetadataQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._createAssignmentConfigurationCommand = createAssignmentConfigurationCommand ?? throw new ArgumentNullException(nameof(createAssignmentConfigurationCommand));
            this._updateAssignmentConfigurationCommand = updateAssignmentConfigurationCommand ?? throw new ArgumentNullException(nameof(updateAssignmentConfigurationCommand));
            this._eventExistsQuery = eventExistsQuery ?? throw new ArgumentNullException(nameof(eventExistsQuery));
            this._deleteEventDecisionCommand = deleteEventDecisionCommand ?? throw new ArgumentNullException(nameof(deleteEventDecisionCommand));
            this._createProfileAssignmentCommand = createProfileAssignmentCommand ?? throw new ArgumentNullException(nameof(createProfileAssignmentCommand));
            this._createOtherAssignmentCommand = createOtherAssignmentCommand ?? throw new ArgumentNullException(nameof(createOtherAssignmentCommand));

            this._eventDelinquencyIdsQuery = eventDelinquencyIdsQuery ?? throw new ArgumentNullException(nameof(eventDelinquencyIdsQuery));
            this._eventDataCutDelinquencyIdsQuery = eventDataCutDelinquencyIdsQuery ?? throw new ArgumentNullException(nameof(eventDataCutDelinquencyIdsQuery));
            this._eventDecisionLevelsQuery = eventDecisionLevelsQuery ?? throw new ArgumentNullException(nameof(eventDecisionLevelsQuery));
            this._decisionLevelProfileQuery = decisionLevelProfileQuery ?? throw new ArgumentNullException(nameof(decisionLevelProfileQuery));
            this._eventsAssignmentsMetadataQuery = eventsAssignmentsMetadataQuery ?? throw new ArgumentNullException(nameof(eventsAssignmentsMetadataQuery));
        }

        public void Handle(EventAssignmentCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(EventAssignmentCreateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._eventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.EventId}' does not exist");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var decisionLevels = await this._eventDecisionLevelsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

            foreach (var assigment in message.LevelAssignments)
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

            var entity = _mapper.Map<EventAssignmentModel>(message);

            await this._createAssignmentConfigurationCommand.DispatchAsync(entity, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("Assignments configuration saved");

            if (message.LevelAssignments.Any(x => x.IsFinal == true))
            {
                var assignmentPerformCommand = Command.Create<EventAssignmentPerformCommand>(Guid.NewGuid(), message.CreatedBy);
                assignmentPerformCommand.EventId = message.EventId;
                await _publisher.PublishAsync(assignmentPerformCommand, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Handle(EventAssignmentUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(EventAssignmentUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._eventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Event '{message.Id}' does not exist");
            }

            cancellationToken.ThrowIfCancellationRequested();

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

            var entity = _mapper.Map<EventAssignmentModel>(message);

            await this._updateAssignmentConfigurationCommand.DispatchAsync(entity, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("Assignments configuration saved");

            if (message.LevelAssignments.Any(x => x.IsFinal == true))
            {
                var assignmentPerformCommand = Command.Create<EventAssignmentPerformCommand>(Guid.NewGuid(), message.CreatedBy);
                assignmentPerformCommand.EventId = message.EventId;
                await _publisher.PublishAsync(assignmentPerformCommand, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Handle(EventAssignmentPerformCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(EventAssignmentPerformCommand message, CancellationToken cancellationToken = default)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(20),
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // Get levels for EventId
                IEnumerable<DecisionLevelModel> decisionLevels = await this._eventDecisionLevelsQuery
                    .ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);

                this._logger.LogInformation("Perform assignment for Event - {EventId} with levels - {LevelsIds}", message.EventId, string.Join(", ", decisionLevels.Select(x => x.Id).ToArray()));

                foreach (var level in decisionLevels.OrderBy(x => x.Order))
                {
                    // Delete decicions with DecisionTypeId != null
                    await this._deleteEventDecisionCommand.DispatchAsync(level.Id, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                    // Get profiles  for levelId
                    IEnumerable<DecisionLevelProfileModel> levelProfiles = await this._decisionLevelProfileQuery.ExecuteAsync(level.Id, cancellationToken).ConfigureAwait(false);

                    this._logger.LogInformation("Perform assignment for Level - {LevelId} with Profiles - {ProfileIds}", level.Id, string.Join(", ", levelProfiles.Select(x => x.PropertyProfileId).ToArray()));

                    List<Guid> previousProfiles = new List<Guid>();

                    foreach (var levelProfile in levelProfiles.OrderBy(x => x.Order))
                    {
                        await this._createProfileAssignmentCommand.DispatchAsync(new CreateProfileAssignmentModel
                        {
                            EventId = message.EventId,
                            ProfileId = levelProfile.PropertyProfileId,
                            Order = levelProfile.Order,
                            EventDecisionLevelId = level.Id,
                            PreviousProfiles = previousProfiles,
                            UserAssignment = levelProfile.UsersAssignment.Select(x => new CreateUserAssignmentModel
                            {
                                UserId = x.UserId,
                                AssignmentsCount = x.AssignmentsCount,
                            }).ToList(),
                        }, message.CreatedBy,
                        cancellationToken).ConfigureAwait(false);

                        previousProfiles.Add(levelProfile.PropertyProfileId);
                    }

                    await this._createOtherAssignmentCommand.DispatchAsync(new CreateOtherAssignmentModel
                    {
                        EventDecisionLevelId = level.Id,
                        EventId = message.EventId,
                        LevelProfileIds = levelProfiles.Select(x => x.PropertyProfileId).ToList(),
                    }, message.CreatedBy,
                    cancellationToken).ConfigureAwait(false);
                }

                scope.Complete();

                this._logger.LogInformation("EventAssignmentPerformCommand have finished  for Event - {EventId}", message.EventId);
            }

            var eventMetadataList = await this._eventsAssignmentsMetadataQuery.ExecuteAsync(new[] { message.EventId }, cancellationToken).ConfigureAwait(false);
            var metadata = eventMetadataList.First();

            var evt = ServiceBus.Abstracts.Event.Create<EventAssignedEvent>(message.EventId, message.CreatedBy);

            evt.NLevelUsers = metadata.NLevelUsers;
            evt.FinalLevelUsers = metadata.FinalLevelUsers;

            await this._publisher.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }
    }
}
