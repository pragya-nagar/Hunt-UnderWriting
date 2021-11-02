using System;
using System.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Common.AspNet;
using Synergy.Common.Logging.Configuration;
using Synergy.DataAccess.Abstractions;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Extensions.Configuration;
using Synergy.Underwriting.DAL.Commands;
using Synergy.Underwriting.Services.Host.AppStart;

namespace Synergy.Underwriting.Services.Host
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
            var connectionString = this._configuration.GetConnectionString("DB");

            var runMigrations = this._hostingEnvironment.IsDevelopment() && this._configuration["DB:RunMigrations"] == "true";
            services.RegisterSynergyEncriptionService(this._hostingEnvironment.IsDevelopment(), this._configuration);
            services.RegisterSynergyContext(connectionString, runMigrations);

            services.RegisterUnderwritingCommands();

            services.AddHealthChecks(this._configuration.GetConnectionString("DB"), name: "Database");

            services.AddAutoMapper(new Assembly[]
            {
                Assembly.Load("Synergy.Underwriting.Services"),
                Assembly.Load("Synergy.Underwriting.DAL.Commands"),
            });

            this.RegisterServices(services);
        }

#pragma warning disable CA1822 // Mark members as static
        public void Configure(IApplicationBuilder app,
#pragma warning restore CA1822 // Mark members as static
            AutoMapper.IMapper mapper)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseStartupLogging();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            app.UseServiceBus();

            app.UseHealthCheck();
            app.UseVersion();
        }

        private void RegisterServices(IServiceCollection services)
        {
            var isDevelopment = this._hostingEnvironment.IsDevelopment();

            services
                .AddDefaultApiContext()
                .AddSerilogLogging(this._configuration)
                .AddServiceBus(this._configuration, isDevelopment)
                .AddFileStorage(this._configuration, isDevelopment)
                .AddServices();
        }
    }
}
