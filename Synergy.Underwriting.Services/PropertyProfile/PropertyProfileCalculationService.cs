using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Messages;
using Synergy.ServiceBus.Messages.Events;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Models.Results;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands.PropertyProfile;

namespace Synergy.Underwriting.Services.PropertyProfile
{
    public class PropertyProfileCalculationService : IMessageHandler<ETLProcessingFinishedEvent>, IMessageHandler<CalculateEventPropertyProfileCommand>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IPublishMessage _serviceBus;
        private readonly GetDelinquencyListByEventIdQuery _getDelinquencyListByEventIdQuery;
        private readonly GetPropertyProfileRuleByIdQuery _getPropertyProfileRuleRulesByProfileRuleIdQuery;
        private readonly IBulkCreatePropertyProfileDelinquencyCommand _bulkCreatePropertyProfileDelinquencyCommand;
        private readonly IDeletePropertyProfileDelinquencyCommand _deletePropertyProfileDelinquencyCommand;
        private readonly GetEtlDelinquencyListByEventIdQuery _getEtlDelinquencyListByEventIdQuery;
        private readonly IDeleteEtlDelinquencyCommand _deleteEtlDelinquencyCommand;
        private readonly GetEventPropertyProfileQuery _getEventPropertyProfileQuery;
        private readonly IChangeEventFreezeStatusCommand _changeEventFreezeStatusCommand;
        private readonly EventsAssignmentsMetadataQuery _eventsAssignmentsMetadataQuery;

        public PropertyProfileCalculationService(ILogger<PropertyProfileService> logger,
            IMapper mapper,
            IPublishMessage serviceBus,
            GetDelinquencyListByEventIdQuery getDelinquencyListByEventIdQuery,
            GetPropertyProfileRuleByIdQuery getPropertyProfileRuleRulesByProfileRuleIdQuery,
            IBulkCreatePropertyProfileDelinquencyCommand bulkCreatePropertyProfileDelinquencyCommand,
            IDeletePropertyProfileDelinquencyCommand deletePropertyProfileDelinquencyCommand,
            GetEtlDelinquencyListByEventIdQuery getEtlDelinquencyListByEventIdQuery,
            IDeleteEtlDelinquencyCommand deleteEtlDelinquencyCommand,
            GetEventPropertyProfileQuery getEventPropertyProfileQuery,
            IChangeEventFreezeStatusCommand changeEventFreezeStatusCommand,
            EventsAssignmentsMetadataQuery eventsAssignmentsMetadataQuery)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            this._getDelinquencyListByEventIdQuery = getDelinquencyListByEventIdQuery ?? throw new ArgumentNullException(nameof(getDelinquencyListByEventIdQuery));
            this._getPropertyProfileRuleRulesByProfileRuleIdQuery = getPropertyProfileRuleRulesByProfileRuleIdQuery ?? throw new ArgumentNullException(nameof(getPropertyProfileRuleRulesByProfileRuleIdQuery));
            this._bulkCreatePropertyProfileDelinquencyCommand = bulkCreatePropertyProfileDelinquencyCommand ?? throw new ArgumentNullException(nameof(bulkCreatePropertyProfileDelinquencyCommand));
            this._deletePropertyProfileDelinquencyCommand = deletePropertyProfileDelinquencyCommand ?? throw new ArgumentNullException(nameof(deletePropertyProfileDelinquencyCommand));
            this._getEtlDelinquencyListByEventIdQuery = getEtlDelinquencyListByEventIdQuery ?? throw new ArgumentNullException(nameof(getEtlDelinquencyListByEventIdQuery));
            this._deleteEtlDelinquencyCommand = deleteEtlDelinquencyCommand ?? throw new ArgumentNullException(nameof(deleteEtlDelinquencyCommand));
            this._getEventPropertyProfileQuery = getEventPropertyProfileQuery ?? throw new ArgumentNullException(nameof(getEventPropertyProfileQuery));
            this._changeEventFreezeStatusCommand = changeEventFreezeStatusCommand ?? throw new ArgumentNullException(nameof(changeEventFreezeStatusCommand));
            this._eventsAssignmentsMetadataQuery = eventsAssignmentsMetadataQuery ?? throw new ArgumentNullException(nameof(eventsAssignmentsMetadataQuery));
        }

        public void Handle(CalculateEventPropertyProfileCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(CalculateEventPropertyProfileCommand message, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<DelinquencyCalculationModel> delinquencyList = null;

            switch (message.Type)
            {
                case PropertyProfileCalculationTriggerType.Etl:
                    // Add logic to get delinquency for event for ETL here
                    delinquencyList = await this._getEtlDelinquencyListByEventIdQuery
                        .ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
                    break;
                case PropertyProfileCalculationTriggerType.ProfileChange:
                    // Add logic to get delinquency for even here
                    delinquencyList = await this._getDelinquencyListByEventIdQuery
                        .ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
                    break;
            }

            if (message.Type == PropertyProfileCalculationTriggerType.Etl)
            {
                await this._deleteEtlDelinquencyCommand
                    .DispatchAsync(message.EventId, message.CreatedBy, cancellationToken).ConfigureAwait(false);
            }

            if (delinquencyList?.Any() == false || message.PropertyProfileIds?.Any() == false)
            {
                this._logger.LogInformation("No Delinquency or profiles found for event - {EventId}.", message.EventId);
                await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = new List<Guid> { message.EventId }, NeedToFreeze = false }, message.CreatedBy).ConfigureAwait(false);

                return;
            }

            this._logger.LogInformation("{DelinquencyCount} delinquency found for event - {EventId}.}", delinquencyList.Count(), message.EventId);

            foreach (Guid propertyProfileId in message.PropertyProfileIds)
            {
                // Delete old property to profile assigment
                await this._deletePropertyProfileDelinquencyCommand.DispatchAsync(new DeleteDelinquencyPropertyProfileModel
                {
                    EventId = message.EventId,
                    ProfileId = propertyProfileId,
                    DelinquencyIds = delinquencyList.Select(x => x.Id).ToArray(),
                }, message.CreatedBy,
                cancellationToken).ConfigureAwait(false);

                // Calculate profile
                await this.CalculateProfile(propertyProfileId, delinquencyList, message.EventId, message.CreatedBy).ConfigureAwait(false);
            }
        }

        public void Handle(ETLProcessingFinishedEvent message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(ETLProcessingFinishedEvent message, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EventPropertyProfileModel> eventProfileList = await this._getEventPropertyProfileQuery.ExecuteAsync(message.CreatedOn, cancellationToken).ConfigureAwait(false);

            if (eventProfileList.Any() == false)
            {
                this._logger.LogInformation("No profiles for calculating MessageId = {MessageId}, CreatedOn - {CreatedOn}", message.Id, message.CreatedOn);
                return;
            }

            this._logger.LogInformation("ETLProcessingFinishedEvent for EventIds - {EventIds} ", string.Join(", ", eventProfileList.Select(x => x.EventId).ToArray()));

            await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = eventProfileList.Select(x => x.EventId).Distinct(), NeedToFreeze = true }, message.CreatedBy).ConfigureAwait(false);

            this._logger.LogInformation("ETLProcessingFinishedEvent events with EventIds - {EventIds} freezed", string.Join(", ", eventProfileList.Select(x => x.EventId).ToArray()));

            foreach (var item in eventProfileList)
            {
                CalculateEventPropertyProfileCommand calculateEventPropertyProfileCommand = Command.Create<CalculateEventPropertyProfileCommand>(Guid.NewGuid(), message.CreatedBy);

                calculateEventPropertyProfileCommand.EventId = item.EventId;
                calculateEventPropertyProfileCommand.PropertyProfileIds = item.PropertyProfileIds;
                calculateEventPropertyProfileCommand.Type = PropertyProfileCalculationTriggerType.Etl;

                await this._serviceBus.PublishAsync(calculateEventPropertyProfileCommand, cancellationToken).ConfigureAwait(false);
            }

            var eventIds = eventProfileList.Select(x => x.EventId).Distinct();
            var eventMetadataList = await this._eventsAssignmentsMetadataQuery.ExecuteAsync(eventIds, cancellationToken).ConfigureAwait(false);
            foreach (var metadata in eventMetadataList)
            {
                var @event = ServiceBus.Abstracts.Event.Create<EventUpdatedEvent>(metadata.Id, message.CreatedBy);

                @event.Number = metadata.Number;
                @event.TypeId = metadata.TypeId;
                @event.StateId = metadata.StateId;
                @event.SaleDate = metadata.SaleDate;
                @event.FundingDate = metadata.FundingDate;
                @event.DepartmentUserIds = metadata.DepartmentUserIds;
                @event.ManualDelinquencyCount = eventMetadataList.Where(x => x.Id == metadata.Id).Select(x => x.ManualDelinquencyCount).FirstOrDefault();

                await this._serviceBus.PublishAsync(@event, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task CalculateProfile(Guid profileId, IEnumerable<DelinquencyCalculationModel> delinquencyList, Guid eventId, Guid createdBy, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<PropertyProfileRuleModel> rules = await this._getPropertyProfileRuleRulesByProfileRuleIdQuery
               .ExecuteAsync(profileId, cancellationToken).ConfigureAwait(false);
            if (rules?.Any() == false)
            {
                this._logger.LogInformation("No rules found for ProfileId - {ProfileId}", profileId);
                await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = new List<Guid> { eventId }, NeedToFreeze = false }, createdBy).ConfigureAwait(false);

                return;
            }

            this._logger.LogInformation("Found rules - {rulesIds} for ProfileId - {ProfileId}", string.Join(", ", rules.Select(x => x.Id).ToArray()), profileId);

            var profileDelinquencyList = new List<CreatePropertyProfileDelinquencyModel>();
            foreach (var delinquencyitem in delinquencyList)
            {
                foreach (var ruleItem in rules)
                {
                    List<Func<DelinquencyCalculationModel, bool>> funcRules = new List<Func<DelinquencyCalculationModel, bool>>();

                    foreach (var item in ruleItem.Items)
                    {
                        if ((int)item.Field < 4)
                        {
                            switch (item.Field)
                            {
                                case PropertyProfileRuleField.GeneralLandUseCode:
                                    if (item.Logic == PropertyProfileLogicType.Include)
                                    {
                                        funcRules.Add(x =>
                                        {
                                            return item.Values.Any(v =>
                                                int.Parse(v, CultureInfo.InvariantCulture) == x.GeneralLandUseCodeId);
                                        });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.Exclude)
                                    {
                                        funcRules.Add(x =>
                                        {
                                            return item.Values.All(v =>
                                                int.Parse(v, CultureInfo.InvariantCulture) != x.GeneralLandUseCodeId);
                                        });
                                    }

                                    break;

                                case PropertyProfileRuleField.InternalLandUseCode:
                                    if (item.Logic == PropertyProfileLogicType.Include)
                                    {
                                        funcRules.Add(x =>
                                        {
                                            return item.Values.Any(v =>
                                                int.Parse(v, CultureInfo.InvariantCulture) == x.InternalLandUseCodeId);
                                        });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.Exclude)
                                    {
                                        funcRules.Add(x =>
                                        {
                                            return item.Values.All(v =>
                                                int.Parse(v, CultureInfo.InvariantCulture) != x.InternalLandUseCodeId);
                                        });
                                    }

                                    break;

                                case PropertyProfileRuleField.LandUseCodes:
                                    if (item.Logic == PropertyProfileLogicType.Include)
                                    {
                                        funcRules.Add(x =>
                                        {
                                            return item.Values.Any(v => v?.ToLower(CultureInfo.InvariantCulture) == x.LandUseCode?.ToLower(CultureInfo.InvariantCulture));
                                        });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.Exclude)
                                    {
                                        funcRules.Add(x =>
                                        {
                                            return item.Values.Any(v => v?.ToLower(CultureInfo.InvariantCulture) != x.LandUseCode?.ToLower(CultureInfo.InvariantCulture));
                                        });
                                    }

                                    break;
                            }
                        }
                        else
                        {
                            List<decimal> values = item.Values.ToList().ConvertAll(x => decimal.Parse(x, CultureInfo.InvariantCulture));

                            switch (item.Field)
                            {
                                case PropertyProfileRuleField.AssessedValue:
                                    if (item.Logic == PropertyProfileLogicType.LessThan)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.AssessedValue < value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThan)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.AssessedValue > value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThanOrEqual)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.AssessedValue >= value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.LessThanOrEqual)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.AssessedValue <= value); });
                                    }

                                    break;

                                case PropertyProfileRuleField.LTV:

                                    var fldLTVPersentValues = values.Select(x => x > 0 ? x / 100 : 0).ToList();

                                    if (item.Logic == PropertyProfileLogicType.LessThan)
                                    {
                                        funcRules.Add(x => { return fldLTVPersentValues.Any(value => x.LTVPercent < value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThan)
                                    {
                                        funcRules.Add(x => { return fldLTVPersentValues.Any(value => x.LTVPercent > value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThanOrEqual)
                                    {
                                        funcRules.Add(x => { return fldLTVPersentValues.Any(value => x.LTVPercent >= value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.LessThanOrEqual)
                                    {
                                        funcRules.Add(x => { return fldLTVPersentValues.Any(value => x.LTVPercent <= value); });
                                    }

                                    break;

                                case PropertyProfileRuleField.RULTV:

                                    var fldRULTVPersentValues = values.Select(x => x > 0 ? x / 100 : 0).ToList();

                                    if (item.Logic == PropertyProfileLogicType.LessThan)
                                    {
                                        funcRules.Add(x => { return fldRULTVPersentValues.Any(value => x.RULTVPercent < value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThan)
                                    {
                                        funcRules.Add(x => { return fldRULTVPersentValues.Any(value => x.RULTVPercent > value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThanOrEqual)
                                    {
                                        funcRules.Add(x => { return fldRULTVPersentValues.Any(value => x.RULTVPercent >= value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.LessThanOrEqual)
                                    {
                                        funcRules.Add(x => { return fldRULTVPersentValues.Any(value => x.RULTVPercent <= value); });
                                    }

                                    break;

                                case PropertyProfileRuleField.TotalDueAmount:
                                    if (item.Logic == PropertyProfileLogicType.LessThan)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.TotalAmountDue < value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThan)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.TotalAmountDue > value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.GreaterThanOrEqual)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.TotalAmountDue >= value); });
                                    }

                                    if (item.Logic == PropertyProfileLogicType.LessThanOrEqual)
                                    {
                                        funcRules.Add(x => { return values.Any(value => x.TotalAmountDue <= value); });
                                    }

                                    break;
                            }
                        }
                    }

                    if (funcRules.Any(x => x(delinquencyitem) == false))
                    {
                        continue;
                    }

                    profileDelinquencyList.Add(new CreatePropertyProfileDelinquencyModel
                    {
                        DelinquencyId = delinquencyitem.Id,
                        PropertyProfileId = profileId,
                    });
                }
            }

            profileDelinquencyList =
                profileDelinquencyList.GroupBy(x => x.DelinquencyId).Select(x => x.First()).ToList();

            await this._bulkCreatePropertyProfileDelinquencyCommand
                .DispatchAsync(profileDelinquencyList, createdBy, cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("Assigned {DelinquencyCount} delinquencies for profile id - '{ProfileId}'", profileDelinquencyList.Count, profileId);

            await this._changeEventFreezeStatusCommand.DispatchAsync(new FreezeEventStatusModel { EventIds = new List<Guid> { eventId }, NeedToFreeze = false }, createdBy).ConfigureAwait(false);

            this._logger.LogInformation("Event with id {EventId} unfreezed'", eventId);
        }
    }
}
