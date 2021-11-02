using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands.PropertyProfile;

namespace Synergy.Underwriting.Services.PropertyProfile
{
    public class PropertyProfileRuleService : IMessageHandler<PropertyProfileRuleCreateCommand>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly ICreatePropertyProfileRuleCommand _createPropertyProfileRuleCommand;

        public PropertyProfileRuleService(ILogger<PropertyProfileRuleService> logger,
            IMapper mapper,
            ICreatePropertyProfileRuleCommand createPropertyProfileRuleCommand)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._createPropertyProfileRuleCommand = createPropertyProfileRuleCommand ?? throw new ArgumentNullException(nameof(createPropertyProfileRuleCommand));
        }

        public void Handle(PropertyProfileRuleCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(PropertyProfileRuleCreateCommand message, CancellationToken cancellationToken = default)
        {
            var propertyProfileRule = _mapper.Map<CreatePropertyProfileRuleModel>(message);
            await this._createPropertyProfileRuleCommand.DispatchAsync(propertyProfileRule, message.CreatedBy, cancellationToken).ConfigureAwait(false);
            this._logger.LogInformation("Created Property Profile Rule '{Id}'", propertyProfileRule.Id);
        }
    }
}
