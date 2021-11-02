using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Synergy.Common.AspNet;
using Synergy.Common.AspNet.Middleware;
using Synergy.Common.Logging.Configuration;
using Synergy.Common.Security.Extensions;
using Synergy.ServiceBus.Extensions.Configuration;
using Synergy.Underwriting.Domain.Validators;

namespace Synergy.Underwriting.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            this._hostingEnvironment = env;
            this._configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc(options =>
                {
                    options.Filters.Add(new AuthorizeFilter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                })
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<AssignmentLevelCreateArgsValidator>());

            services.AddCors("synergy");

            services.AddHealthChecks(this._configuration.GetConnectionString("DB"), "Database");

            services.AddSwagger("Synergy.Underwriting.API");

            services.AddAutoMapper(new Assembly[]
            {
                Assembly.Load("Synergy.Underwriting.API"),
                Assembly.Load("Synergy.Underwriting.Domain"),
                Assembly.Load("Synergy.Underwriting.DAL.Queries.Original"),
                Assembly.Load("Synergy.DataAccess.Abstractions"),
                Assembly.Load("Synergy.DataAccess.Dictionaries.Queries"),
            });

            this.RegisterServices(services);
        }

        public void Configure(IApplicationBuilder app, AutoMapper.IMapper mapper)
        {
            app.UseStartupLogging();
            app.UseVersion();

            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            if (this._hostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseCorrelationLogging();
            app.UseCors("synergy");
            app.UseHealthCheck();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseOperationContext();
            app.UseMvc();
            app.UseSwagger("Synergy Underwriting API V1");
        }

        private void RegisterServices(IServiceCollection services)
        {
            var isDevelopment = this._hostingEnvironment.IsDevelopment();

            services.AddDefaultApiContext()
                .AddSerilogLogging(this._configuration)
                .AddAuth(this._configuration, isDevelopment)
                .AddServiceBus(this._configuration, isDevelopment)
                .AddDefaultFilters()
                .AddFileStorage(this._configuration, isDevelopment)
                .AddDomainServices(this._configuration, isDevelopment)
                .AddDomainValidators();
        }

#pragma warning disable CA1812 // Startup.OperationIdFilter is an internal class that is apparently never instantiated
        private class OperationIdFilter : IOperationFilter
#pragma warning restore CA1812 // Startup.OperationIdFilter is an internal class that is apparently never instantiated
        {
            public void Apply(Operation operation, OperationFilterContext context)
            {
                operation.OperationId = context.MethodInfo.Name + context.MethodInfo.GetParameters()
                                            .Aggregate("_", (acc, cur) => cur.ParameterType == typeof(CancellationToken)
                                                ? acc
                                                : acc + "_" + cur.ParameterType.Name);
            }
        }
    }
}
