using System;
using System.Globalization;
using Amazon;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Synergy.Common.Aws.Extensions;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.Common.FileStorage.AmazonS3;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.ServiceEvents;
using Synergy.ServiceBus.Amazon;
using Synergy.ServiceBus.Extensions.Configuration;
using Synergy.ServiceBus.Messages;
using Synergy.ServiceBus.Messages.Events;
using Synergy.ServiceBus.RabbitMq;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Attachment;
using Synergy.Underwriting.Models.Commands.Comment;
using Synergy.Underwriting.Models.Commands.Event;
using Synergy.Underwriting.Models.Commands.EventAssignment;
using Synergy.Underwriting.Models.Commands.PropertyProfile;
using Synergy.Underwriting.Services.Event;
using Synergy.Underwriting.Services.PropertyProfile;
using Synergy.Underwriting.Services.ReviewReport;

namespace Synergy.Underwriting.Services.Host.AppStart
{
    internal static class ServiceRegistration
    {
        public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (isDevelopment)
            {
                services.AddServiceBus<Synergy.ServiceBus.RabbitMq.MessageBus, RabbitMQConfig>(builder =>
                {
                    builder.Configure(configuration, "ServiceBus:RabbitMQ");

                    AddSubscriptions(builder);
                });
            }
            else
            {
                services.AddServiceBus<Synergy.ServiceBus.Amazon.MessageBus, AWSMessageBusConfig>(builder =>
                {
                    builder.Configure(x =>
                    {
                        configuration.Bind("AwsMessageBus", x);

                        x.TopicName = configuration["TopicName"];
                        x.QueueName = configuration["underwriting:QueueName"];
                        x.Region = configuration.GetRegionEndPoint();
                    });

                    AddSubscriptions(builder);
                });

                services.AddLargeMessageSerializer();
            }

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
        {
            if (isDevelopment)
            {
                var minioEndpointUrl = configuration["MinioEndpointUrl"];
                var minioAccessKey = configuration["MinioAccessKey"];
                var minioSecretKey = configuration["MinioSecretKey"];
                var bucketName = configuration["MinioBucketName"];

                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.USEast1,
                    ServiceURL = minioEndpointUrl,
                    ForcePathStyle = true,
                };

                services.AddTransient<IAmazonS3>(_ => new AmazonS3Client(minioAccessKey, minioSecretKey, config));
                services.AddTransient<IFileStorage>(provider => new AmazonS3Storage(bucketName, provider.GetService<IAmazonS3>()));
            }
            else
            {
                var bucketName = configuration["BucketName"];

                services.AddTransient<IAmazonS3>(_ => new AmazonS3Client(configuration.GetRegionEndPoint()));
                services.AddTransient<IFileStorage>(provider => new AmazonS3Storage(bucketName, provider.GetService<IAmazonS3>()));
            }

            return services;
        }

        private static void AddSubscriptions(IHandlerRegistrationBuilder builder)
        {
            builder.Subscribe<EventService, EventCreateCommand>();
            builder.Subscribe<EventService, EventUpdateCommand>();
            builder.Subscribe<EventService, AssignmentLevelCreateCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(10), DisableParallelProcessing = true });
            builder.Subscribe<EventService, AssignmentLevelUpdateCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(10), DisableParallelProcessing = true });
            builder.Subscribe<EventService, RulesToEventAttachCommand>();
            builder.Subscribe<EventService, RulesToEventUpdateCommand>();
            builder.Subscribe<EventService, AttachmentCreateCommand>();
            builder.Subscribe<EventService, SetEventLockStatusCommand>();
            builder.Subscribe<EventService, EventAttachmentDeleteCommand>();
            builder.Subscribe<EventDumpService, EventDumpFileCreateCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(15), DisableParallelProcessing = true });

            builder.Subscribe<BidService, BidCreateCommand>();
            builder.Subscribe<BidService, BidUpdateCommand>();
            builder.Subscribe<BidService, BidDeleteCommand>();
            builder.Subscribe<BidImportService, BidFileProcessCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(10), DisableParallelProcessing = true });
            builder.Subscribe<BidExportService, BidExportFileCreateCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(10), DisableParallelProcessing = true });

            builder.Subscribe<ResultService, ResultBulkCreateCommand>();
            builder.Subscribe<ResultService, ResultBulkUpdateCommand>();
            builder.Subscribe<ResultImportService, ResultFileProcessCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(10), DisableParallelProcessing = true });

            builder.Subscribe<PropertyService, DelinquencyCommentCreateCommand>();
            builder.Subscribe<PropertyService, DelinquencyCommentUpdateCommand>();
            builder.Subscribe<PropertyService, DelinquencyCommentDeleteCommand>();
            builder.Subscribe<CommentImportService, CommentFileProcessCommand>();

            builder.Subscribe<PropertyService, PropertyUpdateCommand>();
            builder.Subscribe<PropertyService, PropertyAttachmentCreateCommand>();
            builder.Subscribe<PropertyService, MakeDecisionCommand>();
            builder.Subscribe<PropertyService, PropertyAttachmentDeleteCommand>();

            builder.Subscribe<EventDataCutService, RuleCreateCommand>();
            builder.Subscribe<EventDataCutService, ApplyRulesCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(10), DisableParallelProcessing = true });

            builder.Subscribe<PropertyProfileRuleService, PropertyProfileRuleCreateCommand>();
            builder.Subscribe<PropertyProfileService, PropertyProfileCreateCommand>();
            builder.Subscribe<PropertyProfileService, PropertyProfileUpdateCommand>();

            builder.Subscribe<MailMergeService, MailMergeCommand>(new HandleOptions() { IsTerminal = false });
            builder.Subscribe<MailMergeService, MailMergeFinishedEvent>();

            builder.Subscribe<PropertyProfileRuleService, PropertyProfileRuleCreateCommand>();
            builder.Subscribe<PropertyProfileService, PropertyProfileCreateCommand>();
            builder.Subscribe<PropertyProfileService, PropertyProfileUpdateCommand>();
            builder.Subscribe<EventAssignmentService, EventAssignmentCreateCommand>();
            builder.Subscribe<EventAssignmentService, EventAssignmentUpdateCommand>();
            builder.Subscribe<EventAssignmentService, EventAssignmentPerformCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(20), DisableParallelProcessing = true });
            builder.Subscribe<PropertyProfileCalculationService, CalculateEventPropertyProfileCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(10), DisableParallelProcessing = true });
            builder.Subscribe<PropertyProfileCalculationService, ETLProcessingFinishedEvent>();
            builder.Subscribe<EventDumpService, RulesDumpFileCreateCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(5), DisableParallelProcessing = true });
            builder.Subscribe<EventReviewReportService, ReviewDumpCreateCommand>(new HandleOptions { ExecutionTimeout = TimeSpan.FromMinutes(20), DisableParallelProcessing = true });
            builder.SubscribeToServiceEvent<NotificationService, HandlerStartedEvent>();
            builder.SubscribeToServiceEvent<NotificationService, HandlerSuccessEvent>();
            builder.SubscribeToServiceEvent<NotificationService, HandlerExceptionEvent>();
            builder.SubscribeToServiceEvent<NotificationService, DeadMessageEvent>();
            builder.SubscribeToServiceEvent<NotificationService, HandlerDiscardedEvent>();
            builder.SubscribeToServiceEvent<NotificationService, HandlerPostedForProcessingEvent>();
            builder.SubscribeToServiceEvent<NotificationService, MessageReceivedEvent>();
        }
    }
}
