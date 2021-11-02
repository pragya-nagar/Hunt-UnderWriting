using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Synergy.Common.Exceptions;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services
{
    public class BidService :
        IMessageHandler<BidCreateCommand>,
        IMessageHandler<BidUpdateCommand>,
        IMessageHandler<BidDeleteCommand>
    {
        private readonly IMapper _mapper;

        private readonly ICreateBidCommand _createBidCommand;
        private readonly IUpdateBidCommand _updateBidCommand;
        private readonly IDeleteBidCommand _deleteBidCommand;
        private readonly IRefreshResultToBidRelationCommand _refreshResultToBidRelationCommand;

        private readonly GetBidByIdQuery _getBidByIdQuery;
        private readonly GetBidsQuery _getBidsQuery;
        private readonly GetBidByNumberQuery _getBidByNumberQuery;
        private readonly CheckEventExistsQuery _checkEventExistsQuery;

        public BidService(IMapper mapper,
                          ICreateBidCommand createBidCommand,
                          IUpdateBidCommand updateBidCommand,
                          IDeleteBidCommand deleteBidCommand,
                          IRefreshResultToBidRelationCommand refreshResultToBidRelationCommand,
                          GetBidByIdQuery getBidByIdQuery,
                          GetBidsQuery getBidsQuery,
                          GetBidByNumberQuery getBidByNumberQuery,
                          CheckEventExistsQuery checkEventExistsQuery)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            this._createBidCommand = createBidCommand ?? throw new ArgumentNullException(nameof(createBidCommand));
            this._updateBidCommand = updateBidCommand ?? throw new ArgumentNullException(nameof(updateBidCommand));
            this._deleteBidCommand = deleteBidCommand ?? throw new ArgumentNullException(nameof(deleteBidCommand));
            this._refreshResultToBidRelationCommand = refreshResultToBidRelationCommand ?? throw new ArgumentNullException(nameof(refreshResultToBidRelationCommand));
            this._getBidByIdQuery = getBidByIdQuery ?? throw new ArgumentNullException(nameof(getBidByIdQuery));
            this._getBidsQuery = getBidsQuery ?? throw new ArgumentNullException(nameof(getBidsQuery));
            this._getBidByNumberQuery = getBidByNumberQuery ?? throw new ArgumentNullException(nameof(getBidByNumberQuery));
            this._checkEventExistsQuery = checkEventExistsQuery ?? throw new ArgumentNullException(nameof(checkEventExistsQuery));
        }

        public void Handle(BidCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(BidCreateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new ModelStateException($"Event '{message.EventId}' does not exist");
            }

            var bid = await this._getBidByNumberQuery.ExecuteAsync((message.EventId, message.Number), cancellationToken).ConfigureAwait(false);
            if (bid != null)
            {
                throw new ModelStateException("Number", $"Bit number '{message.Number}' should be unique for the event '{message.EventId}'");
            }

            var cmd = this._mapper.Map<CreateBidModel>(message);

            await this._createBidCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            await this._refreshResultToBidRelationCommand.DispatchAsync(new RefreshResultToBidRelationModel { Id = message.EventId }, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(BidUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(BidUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var bid = await this._getBidByIdQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);
            if (bid == null)
            {
                throw new NotFoundException($"Bid {message.Id} not found");
            }

            var exists = await this._checkEventExistsQuery.ExecuteAsync(message.EventId, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new ModelStateException($"Event '{message.EventId}' does not exist");
            }

            bid = await this._getBidByNumberQuery.ExecuteAsync((message.EventId, message.Number), cancellationToken).ConfigureAwait(false);
            if (bid != null && bid.Id != message.Id)
            {
                throw new ModelStateException("Number", $"Bid number '{message.Number}' should be unique for the event '{message.EventId}'");
            }

            var cmd = this._mapper.Map<UpdateBidModel>(message);

            await this._updateBidCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            await this._refreshResultToBidRelationCommand.DispatchAsync(new RefreshResultToBidRelationModel { Id = message.EventId }, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(BidDeleteCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(BidDeleteCommand message, CancellationToken cancellationToken = default)
        {
            var bids = await this._getBidsQuery.ExecuteAsync(message.BidIds, cancellationToken).ConfigureAwait(false);
            if (bids == null || bids.Count() != message.BidIds.Count())
            {
                throw new NotFoundException($"Some bids are not found.");
            }

            var deleteCommand = new DeleteBidModel { BidIds = message.BidIds };
            await this._deleteBidCommand.DispatchAsync(deleteCommand, message.CreatedBy, cancellationToken).ConfigureAwait(false);

            var events = bids.Select(x => x.EventId).Distinct().ToList();

            foreach (var evtId in events)
            {
                var refreshCommand = new RefreshResultToBidRelationModel { Id = evtId };
                await this._refreshResultToBidRelationCommand.DispatchAsync(refreshCommand, message.CreatedBy, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
