using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Synergy.Common.Exceptions;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands.PropertyProfile;

namespace Synergy.Underwriting.Services.PropertyProfile
{
    public class PropertyProfileService : IMessageHandler<PropertyProfileCreateCommand>,
                                          IMessageHandler<PropertyProfileUpdateCommand>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPublishMessage _serviceBus;

        private readonly ICreatePropertyProfileCommand _createPropertyProfileCommand;
        private readonly IUpdatePropertyProfileCommand _updatePropertyProfileCommand;
        private readonly IChangeEventFreezeStatusCommand _changeEventFreezeStatusCommand;
        private readonly IDeletePropertyProfileDelinquencyCommand _deletePropertyProfileDelinquencyCommand;

        private readonly GetPropertyProfileByIdQuery _getPropertyProfileByIdQuery;
        private readonly GetEventIdsByStateIdQuery _getEventIdsByStateIdQuery;
        private readonly CheckProfileNameStatesQuery _checkProfileNameStatesQuery;

        public PropertyProfileService(ILogger<PropertyProfileService> logger,
                IMapper mapper,
                IPublishMessage serviceBus,
                ICreatePropertyProfileCommand createPropertyProfileCommand,
                IUpdatePropertyProfileCommand updatePropertyProfileCommand,
                IChangeEventFreezeStatusCommand changeEventFreezeStatusCommand,
                IDeletePropertyProfileDelinquencyCommand deletePropertyProfileDelinquencyCommand,
                GetPropertyProfileByIdQuery getPropertyProfileByIdQuery,
                GetEventIdsByStateIdQuery getEventIdsByStateIdQuery,
                CheckProfileNameStatesQuery checkProfileNameStatesQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._createPropertyProfileCommand = createPropertyProfileCommand ?? throw new ArgumentNullException(nameof(createPropertyProfileCommand));
            this._updatePropertyProfileCommand = updatePropertyProfileCommand ?? throw new ArgumentNullException(nameof(updatePropertyProfileCommand));
            this._changeEventFreezeStatusCommand = changeEventFreezeStatusCommand ?? throw new ArgumentNullException(nameof(changeEventFreezeStatusCommand));
            this._getPropertyProfileByIdQuery = getPropertyProfileByIdQuery ?? throw new ArgumentNullException(nameof(getPropertyProfileByIdQuery));
            this._getEventIdsByStateIdQuery = getEventIdsByStateIdQuery ?? throw new ArgumentNullException(nameof(getPropertyProfileByIdQuery));
            this._checkProfileNameStatesQuery = checkProfileNameStatesQuery ?? throw new ArgumentNullException(nameof(checkProfileNameStatesQuery));
            this._deletePropertyProfileDelinquencyCommand = deletePropertyProfileDelinquencyCommand ?? throw new ArgumentNullException(nameof(deletePropertyProfileDelinquencyCommand));
        }

        public void Handle(PropertyProfileCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(PropertyProfileCreateCommand message, CancellationToken cancellationToken = default)
        {
            var profileStateExist = await this._checkProfileNameStatesQuery.ExecuteAsync((message.Name, message.StateIds, Guid.NewGuid()), cancellationToken).ConfigureAwait(false);

            if (profileStateExist == true)
            {
                throw new ModelStateException("Please note, the same Profile already exist");
            }

            var propertyProfile = _mapper.Map<CreatePropertyProfileModel>(message);
            await this._createPropertyProfileCommand.DispatchAsync(propertyProfile, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            IEnumerable<EventStateModel> eventStates = await this._getEventIdsByStateIdQuery.ExecuteAsync(message.StateIds).ConfigureAwait(false);
            var eventIds = eventStates.Select(x => x.EventId);
            if (eventIds.Any() == true)
            {
                await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = eventIds, NeedToFreeze = true }, message.CreatedBy).ConfigureAwait(false);
                foreach (var eventId in eventIds)
                {
                    var calculateEventPropertyProfileCommand = Command.Create<CalculateEventPropertyProfileCommand>(Guid.NewGuid(), message.CreatedBy);

                    calculateEventPropertyProfileCommand.EventId = eventId;
                    calculateEventPropertyProfileCommand.PropertyProfileIds = new List<Guid>()
                    {
                        message.Id,
                    };
                    calculateEventPropertyProfileCommand.Type = PropertyProfileCalculationTriggerType.ProfileChange;
                    await this._serviceBus.PublishAsync(calculateEventPropertyProfileCommand, cancellationToken).ConfigureAwait(false);
                }
            }

            this._logger.LogInformation("Created Property Profile '{Id}'", propertyProfile.Id);
        }

        public void Handle(PropertyProfileUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(PropertyProfileUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var oldProfile = await this._getPropertyProfileByIdQuery.ExecuteAsync(message.Id).ConfigureAwait(false);
            if (oldProfile == null)
            {
                this._logger.LogInformation("Cant find Property Profile '{Id}'", message.Id);
                return;
            }

            var profileStateExist = await this._checkProfileNameStatesQuery.ExecuteAsync((message.Name, message.StateIds, message.Id), cancellationToken).ConfigureAwait(false);
            if (profileStateExist == true)
            {
                throw new ModelStateException("Please note, the same Profile already exist");
            }

            var propertyProfile = _mapper.Map<UpdatePropertyProfileModel>(message);
            await this._updatePropertyProfileCommand.DispatchAsync(propertyProfile, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            if (oldProfile.StateIds?.Count() != propertyProfile.StateIds?.Count()
                || oldProfile.StateIds.All(propertyProfile.StateIds.Contains) == false
                || oldProfile.RuleIds?.Count() != propertyProfile.PropertyProfileRuleIds?.Count()
                || oldProfile.RuleIds.All(propertyProfile.PropertyProfileRuleIds.Contains) == false
                || (propertyProfile.IsActive && !oldProfile.IsActive))
            {
                IEnumerable<EventStateModel> eventStates = await this._getEventIdsByStateIdQuery.ExecuteAsync(message.StateIds.Union(oldProfile.StateIds)).ConfigureAwait(false);
                var eventIds = eventStates.Where(x => message.StateIds.Contains(x.StateId)).Select(e => e.EventId);

                var oldStates = oldProfile.StateIds.Except(message.StateIds);
                var eventIdsToDelete = eventStates.Where(x => oldStates.Contains(x.StateId)).Select(e => e.EventId);
                await DeleteOldProfileDelinquencies(eventIdsToDelete, oldProfile.Id, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                if (eventIds.Any() == true)
                {
                    await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = eventIds, NeedToFreeze = true }, message.CreatedBy).ConfigureAwait(false);
                    foreach (var eventId in eventIds)
                    {
                        var calculateEventPropertyProfileCommand = Command.Create<CalculateEventPropertyProfileCommand>(Guid.NewGuid(), message.CreatedBy);

                        calculateEventPropertyProfileCommand.EventId = eventId;
                        calculateEventPropertyProfileCommand.PropertyProfileIds = new List<Guid>()
                        {
                            message.Id,
                        };
                        calculateEventPropertyProfileCommand.Type = PropertyProfileCalculationTriggerType.ProfileChange;
                        await this._serviceBus.PublishAsync(calculateEventPropertyProfileCommand, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            this._logger.LogInformation("Updated Property Profile '{Id}'", propertyProfile.Id);
        }

        private async Task DeleteOldProfileDelinquencies(IEnumerable<Guid> eventIds, Guid propertyProfileId, Guid userId, CancellationToken cancellationToken = default)
        {
            await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = eventIds, NeedToFreeze = true }, userId).ConfigureAwait(false);
            foreach (var id in eventIds)
            {
                await this._deletePropertyProfileDelinquencyCommand.DispatchAsync(
                    new DeleteDelinquencyPropertyProfileModel
                    {
                        EventId = id,
                        ProfileId = propertyProfileId,
                        DelinquencyIds = Enumerable.Empty<Guid>(),
                    },
                    userId,
                    cancellationToken).ConfigureAwait(false);
                await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = new List<Guid> { id }, NeedToFreeze = false }, userId).ConfigureAwait(false);
            }
        }
    }
}
