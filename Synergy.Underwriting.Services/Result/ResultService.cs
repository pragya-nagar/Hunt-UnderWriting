using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services
{
    public class ResultService :
        IMessageHandler<ResultBulkCreateCommand>,
        IMessageHandler<ResultBulkUpdateCommand>
    {
        private readonly IMapper _mapper;
        private readonly IBulkCreateResultCommand _bulkCreateResultCommand;
        private readonly IBulkUpdateResultCommand _bulkUpdateResultCommand;
        private readonly IRefreshResultToBidRelationCommand _refreshResultToBidRelationCommand;

        public ResultService(IBulkCreateResultCommand bulkCreateResultCommand,
                             IBulkUpdateResultCommand bulkUpdateResultCommand,
                             IRefreshResultToBidRelationCommand refreshResultToBidRelationCommand,
                             IMapper mapper)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            this._bulkCreateResultCommand = bulkCreateResultCommand ?? throw new ArgumentNullException(nameof(bulkCreateResultCommand));
            this._bulkUpdateResultCommand = bulkUpdateResultCommand ?? throw new ArgumentNullException(nameof(bulkUpdateResultCommand));
            this._refreshResultToBidRelationCommand = refreshResultToBidRelationCommand ?? throw new ArgumentNullException(nameof(refreshResultToBidRelationCommand));
        }

        public void Handle(ResultBulkCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(ResultBulkCreateCommand message, CancellationToken cancellationToken = default)
        {
            var cmd = this._mapper.Map<IEnumerable<CreateResultModel>>(message.List);
            await this._bulkCreateResultCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            await this._refreshResultToBidRelationCommand.DispatchAsync(new RefreshResultToBidRelationModel { Id = message.EventId }, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(ResultBulkUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(ResultBulkUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var cmd = this._mapper.Map<IEnumerable<UpdateResultModel>>(message.List);
            await this._bulkUpdateResultCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            await this._refreshResultToBidRelationCommand.DispatchAsync(new RefreshResultToBidRelationModel { Id = message.EventId }, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }
    }
}
