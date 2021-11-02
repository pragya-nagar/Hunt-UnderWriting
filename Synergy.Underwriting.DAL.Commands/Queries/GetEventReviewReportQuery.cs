using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands.Event;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetEventReviewReportQuery : CollectionQuery<ReviewDumpCreateCommand, EventReviewReportModel>
    {
        private readonly ISynergyContext _synergyContext;
        private readonly ILogger<GetEventReviewReportQuery> _logger;

        public GetEventReviewReportQuery(ISynergyContext synergyContext, ILogger<GetEventReviewReportQuery> logger)
        {
            this._synergyContext = synergyContext ?? throw new ArgumentNullException(nameof(synergyContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<IEnumerable<EventReviewReportModel>> ExecuteAsync(ReviewDumpCreateCommand reviewReportCommand, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation("Start loading property data.");

            // Get events
            List<EventReviewReportModel> events = await _synergyContext.Event.Include(e => e.County)
                .Where(x => x.StateId == reviewReportCommand.StateId && x.SaleDate >= reviewReportCommand.SaleDateFrom && x.SaleDate <= reviewReportCommand.SaleDateTo && x.DeletedOn == null)
                .Select(r => new EventReviewReportModel { EventId = r.Id, EventNumber = r.EventNumber, County = r.County.Name }).ToListAsync().ConfigureAwait(false);
            foreach (EventReviewReportModel currentEvent in events)
            {
                // Get total count at any level
                currentEvent.TotalCount = await this._synergyContext.Delinquency.CountAsync(x => x.EventId == currentEvent.EventId).ConfigureAwait(false);

                // Get count of auto rejected delinquencies
                currentEvent.BulkRejected = await this._synergyContext.Delinquency.CountAsync(x =>
                      x.EventId == currentEvent.EventId
                      && x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive && e.DecisionTypeId == (int)DataAccess.Enum.DecisionType.AutoReject)).ConfigureAwait(false);

                // Get count of auto approved delinquencies
                currentEvent.BulkApproved = await this._synergyContext.Delinquency.CountAsync(x =>
                       x.EventId == currentEvent.EventId
                       && x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive && e.DecisionTypeId == (int)DataAccess.Enum.DecisionType.AutoApprove)
                       && !x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive && e.DecisionTypeId == (int)DataAccess.Enum.DecisionType.AutoReject)).ConfigureAwait(false);

                // Get count of assigned at any level delinquencies
                currentEvent.ReviewAvailable = currentEvent.TotalCount - (currentEvent.BulkApproved + currentEvent.BulkRejected);

                // Get count of assigned at any level delinquencies
                currentEvent.Assigned = await this._synergyContext.Delinquency.CountAsync(x =>
                        x.EventId == currentEvent.EventId
                        && x.Decisions.Any()
                        && !x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)).ConfigureAwait(false);

                // Get count of approved delinquencies at latest level
                currentEvent.Approved = await this._synergyContext.Delinquency.CountAsync(x =>
                        x.EventId == currentEvent.EventId

                        // Exclude autodecision
                        && !x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Approved decissions at latest level
                        && x.Decisions.Any(e => e.DecisionTypeId == (int)DataAccess.Enum.DecisionType.Approve
                                                && !x.Decisions.Any(dd => dd.EventDecisionLevel.Order > e.EventDecisionLevel.Order && dd.DecisionTypeId != null)))
                        .ConfigureAwait(false);

                // Get count of rejected delinquencies at latest level
                currentEvent.Disapproved = await this._synergyContext.Delinquency.CountAsync(x =>
                        x.EventId == currentEvent.EventId

                        // Exclude autodecision
                        && !x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Reject decissions at latest level
                        && x.Decisions.Any(e => e.DecisionTypeId == (int)DataAccess.Enum.DecisionType.Reject
                                        && !x.Decisions.Any(dd => dd.EventDecisionLevel.Order > e.EventDecisionLevel.Order && dd.DecisionTypeId != null)))
                        .ConfigureAwait(false);

                // Get count of research requiered delinquencies at latest level
                currentEvent.Research = await this._synergyContext.Delinquency.CountAsync(x =>
                        x.EventId == currentEvent.EventId

                        // Exclude autodecision
                        && !x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Research decissions at latest level
                        && x.Decisions.Any(e => e.DecisionTypeId == (int)DataAccess.Enum.DecisionType.Research
                                && !x.Decisions.Any(dd => dd.EventDecisionLevel.Order > e.EventDecisionLevel.Order && dd.DecisionTypeId != null)))
                        .ConfigureAwait(false);

                // Get count of unreviewed delinquencies at latest level
                currentEvent.Unreviewed = await this._synergyContext.Delinquency.CountAsync(x =>
                        x.EventId == currentEvent.EventId

                        // Exclude autodecision
                        && !x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Witout any decision
                        && (x.Decisions.Any() && x.Decisions.All(e => e.DecisionTypeId == null)))
                        .ConfigureAwait(false);

                // Calculate count of manual reviewed delinquencies
                currentEvent.ReviewsCompleted = currentEvent.Disapproved + currentEvent.Research + currentEvent.Approved;

                if (currentEvent.Assigned != 0)
                {
                    currentEvent.ReviewedPercent = ((decimal)currentEvent.ReviewsCompleted / currentEvent.Assigned) * 100;
                    currentEvent.UnreviewedPercent = ((decimal)currentEvent.Unreviewed / currentEvent.Assigned) * 100;
                }

                if (currentEvent.TotalCount != 0)
                {
                    currentEvent.AutoDecisionPercent = ((decimal)(currentEvent.BulkApproved + currentEvent.BulkRejected) / currentEvent.TotalCount) * 100;
                    currentEvent.AssignedPercent = ((decimal)currentEvent.Assigned / currentEvent.TotalCount) * 100;
                    currentEvent.UnassignedPercent = ((decimal)(currentEvent.ReviewAvailable - currentEvent.Assigned) / currentEvent.TotalCount) * 100;
                }
            }

            return events;
        }
    }
}
