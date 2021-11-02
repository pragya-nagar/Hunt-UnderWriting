using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Synergy.DataAccess.Abstractions;
using Synergy.Underwriting.DAL.Commands.Commands;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Queries;

namespace Synergy.Underwriting.DAL.Commands
{
    public static class CommandRegistration
    {
        public static void RegisterUnderwritingCommands(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICreateEventCommand, CreateEventCommand>();
            serviceCollection.AddTransient<IUpdateEventCommand, UpdateEventCommand>();
            serviceCollection.AddTransient<IAttachFileToEventCommand, AttachFileToEventCommand>();
            serviceCollection.AddTransient<ICreateEventDecisionLevelCommand, CreateEventDecisionLevelCommand>();
            serviceCollection.AddTransient<IAssignUserToReviewDelinquencyCommand, AssignUserToReviewDelinquencyCommand>();
            serviceCollection.AddTransient<IReassignUserToReviewDelinquencyCommand, ReassignUserToReviewDelinquencyCommand>();
            serviceCollection.AddTransient<ISetUserDecisionCommand, SetUserDecisionCommand>();
            serviceCollection.AddTransient<ICreateSingleDataCutRuleCommand, CreateSingleDataCutRuleCommand>();
            serviceCollection.AddTransient<ICreateDataCutRuleCommand, CreateDataCutRuleCommand>();
            serviceCollection.AddTransient<IAttachRulesToEventCommand, AttachRulesToEventCommand>();
            serviceCollection.AddTransient<IAddRulesToEventCommand, AddRulesToEventCommand>();
            serviceCollection.AddTransient<IUpdatePropertyCommand, UpdatePropertyCommand>();
            serviceCollection.AddTransient<IAttachFileToPropertyCommand, AttachFileToPropertyCommand>();
            serviceCollection.AddTransient<ICreateEventDataCutDecisionsCommand, CreateEventDataCutDecisionsCommand>();
            serviceCollection.AddTransient<IDeleteEventAttachmentCommand, DeleteEventAttachmentCommand>();
            serviceCollection.AddTransient<IDeletePropertyAttachmentCommand, DeletePropertyAttachmentCommand>();
            serviceCollection.AddTransient<IDeleteEventDecisionCommand, DeleteEventDecisionCommand>();

            serviceCollection.AddTransient<ICreateBidCommand, CreateBidCommand>();
            serviceCollection.AddTransient<IUpdateBidCommand, UpdateBidCommand>();
            serviceCollection.AddTransient<IDeleteBidCommand, DeleteBidCommand>();
            serviceCollection.AddTransient<IBulkCreateBidCommand, BulkCreateBidCommand>();

            serviceCollection.AddTransient<IBulkCreateResultCommand, BulkCreateResultCommand>();
            serviceCollection.AddTransient<IBulkUpdateResultCommand, BulkUpdateResultCommand>();
            serviceCollection.AddTransient<IRefreshResultToBidRelationCommand, RefreshResultToBidRelationCommand>();

            serviceCollection.AddTransient<ICreateOperationStatusCommand, CreateOperationStatusCommand>();
            serviceCollection.AddTransient<IUpdateOperationStatusCommand, UpdateOperationStatusCommand>();

            serviceCollection.AddTransient<ICreateDelinquencyCommentCommand, CreateDelinquencyCommentCommand>();
            serviceCollection.AddTransient<IUpdateDelinquencyCommentCommand, UpdateDelinquencyCommentCommand>();
            serviceCollection.AddTransient<IDeleteDelinquencyCommentCommand, DeleteDelinquencyCommentCommand>();
            serviceCollection.AddTransient<IBulkCreateCommentCommand, BulkCreateCommentCommand>();

            serviceCollection.AddTransient<ISetEventLockStatusCommand, SetEventLockStatusCommand>();

            serviceCollection.AddTransient<ICreatePropertyProfileCommand, CreatePropertyProfileCommand>();
            serviceCollection.AddTransient<IUpdatePropertyProfileCommand, UpdatePropertyProfileCommand>();
            serviceCollection.AddTransient<ICreatePropertyProfileRuleCommand, CreatePropertyProfileRuleCommand>();
            serviceCollection.AddTransient<IRemovePropertyProfileDelinquencyCommand, RemovePropertyProfileDelinquencyCommand>();

            serviceCollection.AddTransient<IBulkCreatePropertyProfileDelinquencyCommand, BulkCreatePropertyProfileDelinquencyCommand>();
            serviceCollection.AddTransient<IDeletePropertyProfileDelinquencyCommand, DeletePropertyProfileDelinquencyCommand>();
            serviceCollection.AddTransient<IDeleteEtlDelinquencyCommand, DeleteEtlDelinquencyCommand>();

            serviceCollection.AddTransient<ICreateAssignmentConfigurationCommand, CreateAssignmentConfigurationCommand>();
            serviceCollection.AddTransient<IUpdateAssignmentConfigurationCommand, UpdateAssignmentConfigurationCommand>();
            serviceCollection.AddTransient<ICreateProfileAssignmentCommand, CreateProfileAssignmentCommand>();
            serviceCollection.AddTransient<ICreateOtherAssignmentCommand, CreateOtherAssignmentCommand>();

            serviceCollection.AddTransient<IChangeEventFreezeStatusCommand, ChangeEventFreezeStatusCommand>();

            serviceCollection.AddTransient<EventsAssignmentsMetadataQuery>();

            serviceCollection.AddTransient<GetMailMergePropertyFieldsQuery>();
            serviceCollection.AddTransient<GetMailMergeEventFieldsQuery>();
            serviceCollection.AddTransient<GetMailMergeTemplateQuery>();

            serviceCollection.AddTransient<DelinquencyExistsQuery>();
            serviceCollection.AddTransient<DelinquencyEventLockQuery>();
            serviceCollection.AddTransient<DelinquencyDecisionsQuery>();

            serviceCollection.AddTransient<GetDelinquencyListQuery>();

            serviceCollection.AddTransient<GetBidListQuery>();
            serviceCollection.AddTransient<GetBidByIdQuery>();
            serviceCollection.AddTransient<GetBidsQuery>();
            serviceCollection.AddTransient<GetBidByNumberQuery>();

            serviceCollection.AddTransient<CheckEventExistsQuery>();

            serviceCollection.AddTransient<GetExportEventQuery>();
            serviceCollection.AddTransient<GetExportPropertiesQuery>();
            serviceCollection.AddTransient<DataCutQuery>();
            serviceCollection.AddTransient<GetCountyByEvent>();

            serviceCollection.AddTransient<EventDelinquencyIdsQuery>();
            serviceCollection.AddTransient<EventDataCutDelinquencyIdsQuery>();
            serviceCollection.AddTransient<EventDecisionLevelsQuery>();
            serviceCollection.AddTransient<EventDecisionQuery>();
            serviceCollection.AddTransient<UnderwritingWorkflowDefinitionQuery>();

            serviceCollection.AddTransient<GetPropertyProfileByEventIdQuery>();
            serviceCollection.AddTransient<GetDelinquencyListByEventIdQuery>();
            serviceCollection.AddTransient<GetPropertyProfileRuleByIdQuery>();

            serviceCollection.AddTransient<GetCountyNameQuery>();
            serviceCollection.AddTransient<GetStateAbbreviationQuery>();
            serviceCollection.AddTransient<GetEventTypeQuery>();
            serviceCollection.AddTransient<GetEventNamesByLocationQuery>();
            serviceCollection.AddTransient<GetEventLockStatusQuery>();
            serviceCollection.AddTransient<GetPropertyProfileByIdQuery>();
            serviceCollection.AddTransient<GetEventIdsByStateIdQuery>();
            serviceCollection.AddTransient<GetEtlDelinquencyListByEventIdQuery>();
            serviceCollection.AddTransient<GetEventPropertyProfileQuery>();
            serviceCollection.AddTransient<EventDecisionLevelsProfileQuery>();
            serviceCollection.AddTransient<EventDecisionLevelsQuery>();
            serviceCollection.AddTransient<DecisionLevelProfileQuery>();
            serviceCollection.AddTransient<CheckLevelReviewFinishedQuery>();
            serviceCollection.AddTransient<CheckProfileNameStatesQuery>();
            serviceCollection.AddTransient<GetExportRulesQuery>();
            serviceCollection.AddTransient<GetEventQuery>();
            serviceCollection.AddTransient<GetEventReviewReportQuery>();
            serviceCollection.AddTransient<GetEventPerUserReviewReportQuery>();

            serviceCollection.AddTransient<GetCommentAuthorIdQuery>();
        }
    }
}
