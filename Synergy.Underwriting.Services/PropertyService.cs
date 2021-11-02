using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Amazon.S3;
using Amazon.S3.Transfer;
using AutoMapper;
using Synergy.Common.Exceptions;
using Synergy.DataAccess.Enum;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Messages;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.DAL.Commands.Queries;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Attachment;
using Synergy.Underwriting.Models.Commands.Comment;

namespace Synergy.Underwriting.Services
{
    public class PropertyService :
        IMessageHandler<MakeDecisionCommand>,
        IMessageHandler<PropertyUpdateCommand>,
        IMessageHandler<DelinquencyCommentCreateCommand>,
        IMessageHandler<DelinquencyCommentUpdateCommand>,
        IMessageHandler<DelinquencyCommentDeleteCommand>,
        IMessageHandler<PropertyAttachmentCreateCommand>,
        IMessageHandler<PropertyAttachmentDeleteCommand>
    {
        private readonly IMapper _mapper;
        private readonly IPublishMessage _publisher;

        private readonly ISetUserDecisionCommand _setUserDecisionCommand;
        private readonly IUpdatePropertyCommand _updatePropertyCommand;
        private readonly ICreateDelinquencyCommentCommand _createDelinquencyCommentCommand;
        private readonly IUpdateDelinquencyCommentCommand _updateDelinquencyCommentCommand;
        private readonly IDeleteDelinquencyCommentCommand _deleteDelinquencyCommentCommand;
        private readonly IAttachFileToPropertyCommand _attachFileToPropertyCommand;
        private readonly IDeletePropertyAttachmentCommand _deletePropertyAttachmentCommand;

        private readonly DelinquencyExistsQuery _delinquencyExistsQuery;
        private readonly DelinquencyEventLockQuery _delinquencyEventLockQuery;
        private readonly DelinquencyDecisionsQuery _delinquencyDecisionsQuery;
        private readonly CheckLevelReviewFinishedQuery _checkLevelReviewFinishedQuery;
        private readonly GetCommentAuthorIdQuery _getCommentAuthorIdQuery;
        private readonly IAmazonS3 _client;
        public PropertyService(
            IMapper mapper,
            IPublishMessage publisher,
            ISetUserDecisionCommand setUserDecisionCommand,
            ICreateDelinquencyCommentCommand createDelinquencyCommentCommand,
            IUpdateDelinquencyCommentCommand updateDelinquencyCommentCommand,
            IDeleteDelinquencyCommentCommand deleteDelinquencyCommentCommand,
            IAttachFileToPropertyCommand attachFileToPropertyCommand,
            IUpdatePropertyCommand updatePropertyCommand,
            IDeletePropertyAttachmentCommand deletePropertyAttachmentCommand,
            DelinquencyExistsQuery delinquencyExistsQuery,
            DelinquencyEventLockQuery delinquencyEventLockQuery,
            DelinquencyDecisionsQuery delinquencyDecisionsQuery,
            CheckLevelReviewFinishedQuery checkLevelReviewFinishedQuery,
            GetCommentAuthorIdQuery getCommentAuthorIdQuery,
            IAmazonS3 client)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

            this._setUserDecisionCommand = setUserDecisionCommand ?? throw new ArgumentNullException(nameof(setUserDecisionCommand));
            this._updatePropertyCommand = updatePropertyCommand ?? throw new ArgumentNullException(nameof(updatePropertyCommand));
            this._attachFileToPropertyCommand = attachFileToPropertyCommand ?? throw new ArgumentNullException(nameof(attachFileToPropertyCommand));

            this._createDelinquencyCommentCommand = createDelinquencyCommentCommand ?? throw new ArgumentNullException(nameof(createDelinquencyCommentCommand));
            this._updateDelinquencyCommentCommand = updateDelinquencyCommentCommand ?? throw new ArgumentNullException(nameof(updateDelinquencyCommentCommand));
            this._deleteDelinquencyCommentCommand = deleteDelinquencyCommentCommand ?? throw new ArgumentNullException(nameof(deleteDelinquencyCommentCommand));

            this._deletePropertyAttachmentCommand = deletePropertyAttachmentCommand ?? throw new ArgumentNullException(nameof(deletePropertyAttachmentCommand));

            this._delinquencyExistsQuery = delinquencyExistsQuery ?? throw new ArgumentNullException(nameof(delinquencyExistsQuery));
            this._delinquencyEventLockQuery = delinquencyEventLockQuery ?? throw new ArgumentNullException(nameof(delinquencyEventLockQuery));
            this._delinquencyDecisionsQuery = delinquencyDecisionsQuery ?? throw new ArgumentNullException(nameof(delinquencyDecisionsQuery));
            this._checkLevelReviewFinishedQuery = checkLevelReviewFinishedQuery ?? throw new ArgumentNullException(nameof(checkLevelReviewFinishedQuery));
            this._getCommentAuthorIdQuery = getCommentAuthorIdQuery ?? throw new ArgumentNullException(nameof(getCommentAuthorIdQuery));
            _client = client;
        }

        public void Handle(PropertyUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(PropertyUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._delinquencyExistsQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Delinquency '{message.Id}' does not exist");
            }

            var eventData = await this._delinquencyEventLockQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);
            if (eventData.IsLocked == true)
            {
                throw new NotAcceptableException("You can't update this delinquency, because event is locked");
            }

            var cmd = this._mapper.Map<UpdatePropertyModel>(message);
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await this._updatePropertyCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);

                scope.Complete();
            }
        }

        public void Handle(MakeDecisionCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(MakeDecisionCommand message, CancellationToken cancellationToken = default)
        {
            var exists = await this._delinquencyExistsQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);
            if (exists == false)
            {
                throw new NotFoundException($"Delinquency '{message.Id}' does not exist");
            }

            var eventData = await this._delinquencyEventLockQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);
            if (eventData.IsLocked == true)
            {
                throw new NotAcceptableException("You can't make a decision, because event is locked");
            }

            if (message.Decision == DataAccess.Enum.DecisionType.Reject
                && string.IsNullOrWhiteSpace(message.Comment)
                && eventData.IsRejectReasonReuired)
            {
                throw new NotAcceptableException("Reject reason is required for this event, but it was not provided.");
            }

            var decisions = await this._delinquencyDecisionsQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);

            var decision = decisions.FirstOrDefault(x => x.LevelId == message.LevelId);
            if (decision == null)
            {
                throw new ModelStateException($"Level '{message.LevelId}' does not exist");
            }

            if (decisions.All(x => x.UserId != message.CreatedBy) == true)
            {
                throw new NotAcceptableException("You have no permissions to modify the property");
            }

            if (decision.IsFinal == false && decisions.Any(x => x.Order < decision.Order && x.DecisionTypeId == null))
            {
                throw new NotAcceptableException("The property has no decisions on previous levels");
            }

            await this._setUserDecisionCommand.DispatchAsync(new SetUserDecisionModel
            {
                DecisionId = decision.Id,
                Decision = message.Decision,
                DecisionDate = message.CreatedOn,
                Comment = message.Comment,
            }, message.CreatedBy,
               cancellationToken).ConfigureAwait(false);

            var isLevelReviewCompleted = await this._checkLevelReviewFinishedQuery.ExecuteAsync((message.LevelId, message.CreatedBy), cancellationToken).ConfigureAwait(false);

            var evt = ServiceBus.Abstracts.Event.Create<DelinquencyReviewedEvent>(message.Id, message.CreatedBy);
            evt.EventId = eventData.EventId;
            evt.LevelReviewFinished = isLevelReviewCompleted;
            evt.Order = decision.Order;
            evt.NextReviewerId = decisions.FirstOrDefault(x => x.Order == decision.Order + 1)?.UserId;

            await this._publisher.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(DelinquencyCommentCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(DelinquencyCommentCreateCommand message, CancellationToken cancellationToken = default)
        {
            var cmd = this._mapper.Map<CreateDelinquencyCommentModel>(message);
            await this._createDelinquencyCommentCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(DelinquencyCommentUpdateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(DelinquencyCommentUpdateCommand message, CancellationToken cancellationToken = default)
        {
            var author = await this._getCommentAuthorIdQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);

            if (author == null)
            {
                throw new NotFoundException();
            }

            if (author != message.CreatedBy)
            {
                throw new NotAcceptableException("Only author can alter comment.");
            }

            await this._updateDelinquencyCommentCommand.DispatchAsync(new UpdateDelinquencyCommentModel
                {
                    Id = message.Id,
                    Comment = message.Comment,
                }, message.CreatedBy,
                   cancellationToken)
                .ConfigureAwait(false);
        }

        public void Handle(DelinquencyCommentDeleteCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(DelinquencyCommentDeleteCommand message, CancellationToken cancellationToken = default)
        {
            var author = await this._getCommentAuthorIdQuery.ExecuteAsync(message.Id, cancellationToken).ConfigureAwait(false);

            if (author == null)
            {
                throw new NotFoundException();
            }

            if (author != message.CreatedBy)
            {
                throw new NotAcceptableException("Only author can delete comment.");
            }

            var command = new DeleteDelinquencyCommentModel { Id = message.Id };
            await this._deleteDelinquencyCommentCommand
                .DispatchAsync(command, message.CreatedBy, cancellationToken)
                .ConfigureAwait(false);
        }

        public void Handle(PropertyAttachmentCreateCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(PropertyAttachmentCreateCommand message, CancellationToken cancellationToken = default)
        {
            var cmd = this._mapper.Map<AttachFileToPropertyModel>(message);
            cmd.AttachmentType = PropertyAttachmentType.Attachment;
            cmd.ContentType = "application/octet-stream";

            await this._attachFileToPropertyCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

        public void Handle(PropertyAttachmentDeleteCommand message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(PropertyAttachmentDeleteCommand message, CancellationToken cancellationToken = default)
        {
            var cmd = this._mapper.Map<DeleteAttachmentModel>(message);
            await _deletePropertyAttachmentCommand.DispatchAsync(cmd, message.CreatedBy, cancellationToken).ConfigureAwait(false);
        }

    }
}
