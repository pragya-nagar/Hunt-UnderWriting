using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.ServiceEvents;
using Synergy.ServiceBus.Extensions.Handlers;
using Synergy.ServiceBus.Extensions.Progress;
using Synergy.ServiceBus.Messages;

namespace Synergy.Underwriting.Services
{
    public class NotificationService : ServiceHandler
    {
        private const string ServerErrorMessage = "The operation failed in an unusual way. Please try again later.";

        private readonly IPublishMessage _publisher;
        private readonly ILogger _logger;
        private readonly IProgressScopeAccessor _progressScopeAccessor;

        public NotificationService(IPublishMessage publisher, ILogger<NotificationService> logger, IProgressScopeAccessor progressScopeAccessor)
        {
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._progressScopeAccessor = progressScopeAccessor ?? throw new ArgumentNullException(nameof(progressScopeAccessor));
        }

        public override async Task HandleAsync(HandlerStartedEvent message, CancellationToken cancellationToken)
        {
            if (message.EventArgs.Message is Command command)
            {
                this.LogServiceEvent(message.GetType().Name, message.EventArgs);

                await this.PostStatusAsync(command, HttpStatusCode.Accepted, "Accepted", (int)(this._progressScopeAccessor.Current?.TotalProgress ?? 0), cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task HandleAsync(HandlerSuccessEvent message, CancellationToken cancellationToken)
        {
            if (message.EventArgs.Message is Command command && message.EventArgs.HandlerData?.Options?.IsTerminal != false)
            {
                this.LogServiceEvent(message.GetType().Name, message.EventArgs);

                await this.PostStatusAsync(command, HttpStatusCode.OK, "OK", null, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task HandleAsync(HandlerExceptionEvent message, CancellationToken cancellationToken)
        {
            if (message.EventArgs.Message is Command command)
            {
                this.LogServiceEvent(message.GetType().Name, message.EventArgs);

                var ex = message.Exception;

                if (message.Exception is AggregateException aggregateException)
                {
                    ex = aggregateException.InnerException;
                }

                if (ex is ApplicationException)
                {
                    this._logger.LogError(ex, "Application defined error occured. Message will be deleted.");

                    message.Handled = true;

                    await this.PostStatusAsync(command, HttpStatusCode.BadRequest, ex.Message, null, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await this.PostStatusAsync(command, HttpStatusCode.TemporaryRedirect, "The operation can take more time than usual.", null, cancellationToken).ConfigureAwait(false);

                    this._logger.LogError(ex, "Unhandled error occured. Message can be retried.");
                }
            }
        }

        public override async Task HandleAsync(DeadMessageEvent message, CancellationToken cancellationToken)
        {
            if (message.EventArgs.Message is Command command)
            {
                this.LogServiceEvent(message.GetType().Name, message.EventArgs);

                await this.PostStatusAsync(command, HttpStatusCode.InternalServerError, ServerErrorMessage, null, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task HandleAsync(HandlerDiscardedEvent message, CancellationToken cancellationToken)
        {
            if (message.EventArgs.Message is Command command)
            {
                this.LogServiceEvent(message.GetType().Name, message.EventArgs);

                await this.PostStatusAsync(command, HttpStatusCode.TemporaryRedirect, "Server is busy. The operation can take more time than usual.", null, cancellationToken).ConfigureAwait(false);
            }
        }

        public override Task HandleAsync(HandlerPostedForProcessingEvent message, CancellationToken cancellationToken)
        {
            this.LogServiceEvent(message.GetType().Name, message.EventArgs);

            return Task.CompletedTask;
        }

        public override async Task HandleAsync(MessageReceivedEvent message, CancellationToken cancellationToken)
        {
            if (message.EventArgs.Message is Command command)
            {
                this.LogServiceEvent(message.GetType().Name, message.EventArgs);

                await this.PostStatusAsync(command, HttpStatusCode.Accepted, "Accepted", (int)(this._progressScopeAccessor.Current?.TotalProgress ?? 0), cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task PostStatusAsync(
            Command message,
            HttpStatusCode statusCode,
            string statusMessage,
            int? progress,
            CancellationToken cancellationToken)
        {
            var evt = ServiceBus.Abstracts.Event.Create<OperationStatusEvent>(Guid.NewGuid(), message.CreatedBy);

            evt.Code = statusCode;
            evt.Message = statusMessage;
            evt.Progress = progress;

            await this._publisher.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }

        private void LogServiceEvent(string eventType, ServiceEvent args)
        {
            this._logger.LogInformation("{eventType} event received for message {messageType}. {@messageInfo}",
                eventType,
                args.Message.GetType(),
                new
                {
                    args.ReceiveTimestamp,
                    args.ReceivedCount,
                    args.HandlerData?.HandlerStartedTimestamp,
                    args.HandlerData?.Options,
                    HandlerType = args.HandlerData?.Handler?.GetType(),
                });
        }
    }
}
