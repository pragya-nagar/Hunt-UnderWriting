using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Synergy.DataAccess.Abstractions;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Queries;

namespace Synergy.Underwriting.DAL.Queries.Original
{
    public static class QueryRegistration
    {
        public static void RegisterUnderwritingQueries(this IServiceCollection serviceCollection, string connectionString, IConfiguration configuration, bool runMigration = true, bool isDevelopment = false)
        {
            serviceCollection.RegisterSynergyEncriptionService(isDevelopment, configuration);
            serviceCollection.RegisterSynergyContext(connectionString, runMigration);

            ValidateMapperConfigurations(serviceCollection);

            serviceCollection.AddTransient<IGetEventsQuery, GetEventsQuery>();
            serviceCollection.AddTransient<IGetEventAttachmentQuery, GetEventAttachmentQuery>();
            serviceCollection.AddTransient<IGetDelinquencyQuery, GetDelinquencyQuery>();
            serviceCollection.AddTransient<IGetDataCutRuleFieldsQuery, GetDataCutRuleFieldsQuery>();
            serviceCollection.AddTransient<IGetDataCutLogicTypesQuery, GetDataCutLogicTypesQuery>();
            serviceCollection.AddTransient<IGetDataCutResultTypeQuery, GetDataCutResultTypeQuery>();
            serviceCollection.AddTransient<IGetDataCutRuleQuery, GetDataCutRuleQuery>();
            serviceCollection.AddTransient<IGetStateTaxRatesQuery, GetStateTaxRatesQuery>();
            serviceCollection.AddTransient<IGetEventDataCutDecisionQuery, GetEventDataCutDecisionQuery>();
            serviceCollection.AddTransient<IGetDataCutPropertyQuery, GetDataCutPropertyQuery>();
            serviceCollection.AddTransient<IGetDelinquencyCommentsQuery, GetDelinquencyCommentsQuery>();

            serviceCollection.AddTransient<IGetEventCalculatedFields, GetEventCalculatedFields>();
            serviceCollection.AddTransient<IGetEventDelinquenciesQuery, GetEventDelinquenciesQuery>();
            serviceCollection.AddTransient<IGetEventDecisionLevelQuery, GetEventDecisionLevelQuery>();
            serviceCollection.AddTransient<IGetCountyQuery, GetCountyQuery>();

            serviceCollection.AddTransient<IGetEventAssigmentsCountQuery, GetEventAssigmentsCountQuery>();
        }

        private static void ValidateMapperConfigurations(IServiceCollection serviceCollection)
        {
            var mapper = serviceCollection.BuildServiceProvider().GetService<IMapper>();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
