using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using Amazon;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Synergy.Common.Aws.Extensions;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.DAL.Access.PostgreSQL;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.Common.FileStorage.AmazonS3;
using Synergy.Common.Security;
using Synergy.DataAccess.Dictionaries.Queries;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Amazon;
using Synergy.ServiceBus.Extensions.Configuration;
using Synergy.ServiceBus.RabbitMq;
using Synergy.Underwriting.DAL.Queries.Original;
using Synergy.Underwriting.Domain;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Domain.Validators;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Bid;
using Synergy.Underwriting.Models.EventAssignment;
using Synergy.Underwriting.Models.Property;

namespace Synergy.Underwriting.API
{
    internal static class ServiceRegistrations
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
                });

                services.AddLargeMessageSerializer();
            }

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

        public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
        {
            var connectionString = configuration.GetConnectionString("DB");
            var runMigrations = configuration["DB:RunMigrations"] == "true";

            // for migration in development configuration
            services.RegisterUnderwritingQueries(connectionString, configuration, isDevelopment && runMigrations, isDevelopment);
            services.RegisterDictionariesQueries(connectionString, false);

            services.AddScoped<IDataAccess>(provider => new DAL.Queries.PostgreSQL.DataAccess(provider.GetService<ILoggerFactory>(), connectionString));
            services.AddTransient(typeof(IQueryProvider<>), typeof(QueryProvider<>));

            services.AddTransient<IEventService, EventService>();
            services.AddTransient<IBidService, BidService>();
            services.AddTransient<IDictionaryService, DictionaryService>();
            services.AddTransient<IPropertyService, PropertyService>();
            services.AddTransient<IHistoryService, HistoryService>();
            services.AddTransient<IPropertyProfileService, PropertyProfileService>();
            services.AddTransient<IEventAssignmentsService, EventAssignmentsService>();
            return services;
        }

        public static IServiceCollection AddDomainValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<EventCreateArgs>, EventCreateArgsValidator>();
            services.AddTransient<IValidator<EventUpdateArgs>, EventUpdateArgsValidator>();

            services.AddTransient<IValidator<BidCreateArgs>, BidCreateArgsValidator>();
            services.AddTransient<IValidator<BidUpdateArgs>, BidUpdateArgsValidator>();

            services.AddTransient<IValidator<ResultCreateArgs>, ResultCreateArgsValidator>();

            services.AddTransient<IValidator<AssignmentLevelCreateArgs>, AssignmentLevelCreateArgsValidator>();
            services.AddTransient<IValidator<PropertyUpdateArgs>, PropertyUpdateArgsValidator>();

            return services;
        }
    }
}
