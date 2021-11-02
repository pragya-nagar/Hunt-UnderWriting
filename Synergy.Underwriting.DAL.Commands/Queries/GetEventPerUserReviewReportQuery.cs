using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands.Event;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetEventPerUserReviewReportQuery : CollectionQuery<ReviewDumpCreateCommand, EventPerUserReviewReportModel>
    {
        private readonly ISynergyContext _synergyContext;
        private readonly ILogger<GetEventPerUserReviewReportQuery> _logger;

        public GetEventPerUserReviewReportQuery(ISynergyContext synergyContext, ILogger<GetEventPerUserReviewReportQuery> logger)
        {
            this._synergyContext = synergyContext ?? throw new ArgumentNullException(nameof(synergyContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<IEnumerable<EventPerUserReviewReportModel>> ExecuteAsync(ReviewDumpCreateCommand reviewReportCommand, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation("Start loading Event Per User Review Repor data.");

            IQueryable<EventDecisionLevelUser> query = _synergyContext.EventDecisionLevelUser
                .Include(edlu => edlu.EventDecisionLevel).ThenInclude(edl => edl.Event).ThenInclude(e => e.County)
                .Include(edlu => edlu.User);

            if (reviewReportCommand.IsEventLocked == true)
            {
                query = query.Where(edlu => edlu.EventDecisionLevel.Event.StateId == reviewReportCommand.StateId
                                            && edlu.EventDecisionLevel.Event.IsLocked == true
                                            && edlu.EventDecisionLevel.Event.SaleDate >= reviewReportCommand.SaleDateFrom
                                            && edlu.EventDecisionLevel.Event.SaleDate <= reviewReportCommand.SaleDateTo
                                            && edlu.EventDecisionLevel.Event.DeletedOn == null);
            }
            else
            {
                query = query.Where(edlu => edlu.EventDecisionLevel.Event.StateId == reviewReportCommand.StateId
                                            && edlu.EventDecisionLevel.Event.IsLocked == false
                                            && edlu.EventDecisionLevel.Event.DeletedOn == null);
            }

            List<EventPerUserReviewReportModel> eventUsers = await query.Select(r => new EventPerUserReviewReportModel
            {
                EventId = r.EventDecisionLevel.Event.Id,
                EventNumber = r.EventDecisionLevel.Event.EventNumber,
                County = r.EventDecisionLevel.Event.County.Name,
                ReviewerId = r.UserId,
                ReviewerFirstName = r.User.FirstName,
                ReviewerLastName = r.User.LastName,
                Level = r.EventDecisionLevel.Name,
                LevelId = r.EventDecisionLevelId,
                LevelOrder = r.EventDecisionLevel.Order,
            }).ToListAsync(cancellationToken).ConfigureAwait(false);

            eventUsers = eventUsers.GroupBy(edl => new { edl.ReviewerId, edl.EventId, edl.Level }).Select(g => g.First()).ToList();
            foreach (EventPerUserReviewReportModel eventUser in eventUsers)
            {
                // Get count of assigned to user at specific level delinquencies
                eventUser.Assigned = await _synergyContext.Decision.CountAsync(d =>
                        d.Delinquency.EventId == eventUser.EventId
                        && d.UserId == eventUser.ReviewerId
                        && d.EventDecisionLevelId == eventUser.LevelId
                        && !d.Delinquency.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)).ConfigureAwait(false);

                // Get count of approved delinquencies at specific level
                eventUser.Approved = await this._synergyContext.Decision.CountAsync(d =>
                        d.Delinquency.EventId == eventUser.EventId
                        && d.UserId == eventUser.ReviewerId
                        && !d.Delinquency.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Approved decissions at specific level
                        && d.EventDecisionLevelId == eventUser.LevelId
                        && d.DecisionTypeId == (int)DataAccess.Enum.DecisionType.Approve).ConfigureAwait(false);

                // Get count of rejected delinquencies at specific level
                eventUser.Disapproved = await this._synergyContext.Decision.CountAsync(d =>
                        d.Delinquency.EventId == eventUser.EventId
                        && d.UserId == eventUser.ReviewerId
                        && !d.Delinquency.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Reject decissions at specific level
                        && d.EventDecisionLevelId == eventUser.LevelId
                        && d.DecisionTypeId == (int)DataAccess.Enum.DecisionType.Reject).ConfigureAwait(false);

                // Get count of research requiered delinquencies at specific level
                eventUser.Research = await this._synergyContext.Decision.CountAsync(d =>
                        d.Delinquency.EventId == eventUser.EventId
                        && d.UserId == eventUser.ReviewerId
                        && !d.Delinquency.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Research decissions at specific level
                        && d.EventDecisionLevelId == eventUser.LevelId
                        && d.DecisionTypeId == (int)DataAccess.Enum.DecisionType.Research).ConfigureAwait(false);

                // Get count of unreviewed delinquencies at specific level
                eventUser.Unreviewed = await this._synergyContext.Decision.CountAsync(d =>
                        d.Delinquency.EventId == eventUser.EventId
                        && d.UserId == eventUser.ReviewerId
                        && !d.Delinquency.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)

                        // Witout any decision per level
                        && d.EventDecisionLevelId == eventUser.LevelId
                        && d.DecisionTypeId == null).ConfigureAwait(false);

                // Calculate count of manual reviewed delinquencies
                eventUser.ReviewsCompleted = eventUser.Disapproved + eventUser.Research + eventUser.Approved;

                if (eventUser.Assigned != 0)
                {
                    eventUser.ReviewedPercent = ((decimal)eventUser.ReviewsCompleted / eventUser.Assigned) * 100;
                    eventUser.UnreviewedPercent = ((decimal)eventUser.Unreviewed / eventUser.Assigned) * 100;
                }
            }

            return eventUsers;
        }
    }
}
