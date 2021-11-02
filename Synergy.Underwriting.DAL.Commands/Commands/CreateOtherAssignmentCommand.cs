using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class CreateOtherAssignmentCommand : ICreateOtherAssignmentCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreateOtherAssignmentCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(CreateOtherAssignmentModel model, Guid userId)
        {
            this.DispatchAsync(model, userId).Wait();
        }

        public async Task<int> DispatchAsync(CreateOtherAssignmentModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Guid> profileDeliquencies = await this._context.PropertyProfileDelinquency
                .Where(x => model.LevelProfileIds.Contains(x.PropertyProfileId) &&
                            x.Delinquency.EventId == model.EventId && x.DeletedOn == null)
                .Select(x => x.DelinquencyId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Deliquencies with user decision on current level will be excluded
            List<Guid> deliquenciesIds = await this._context.Delinquency
                .Where(x => x.EventId == model.EventId
                            && !x.EventDataCutDecisions.Any(d => d.EventDataCutStrategy.IsActive == true)
                            && !profileDeliquencies.Contains(x.Id)
                            && !x.Decisions.Any(d => d.EventDecisionLevelId == model.EventDecisionLevelId && d.DecisionTypeId != null)
                            && x.DeletedOn == null)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var userAsigment = await this._context.EventDecisionLevelUser
                .Where(x => x.EventDecisionLevelId == model.EventDecisionLevelId && x.DeletedOn == null &&
                            x.EventDecisionLevelPropertyProfileId == null)
                .Select(x => new
                {
                    x.UserId,
                    x.AssigmentCount,
                }).ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (deliquenciesIds.Any() == false)
            {
                return 0;
            }

            List<Decision> newDecisions = new List<Decision>();
            int skip = 0;
            foreach (var userAssignment in userAsigment)
            {
                var assignmentDeliquencies = deliquenciesIds.Skip(skip).Take(userAssignment.AssigmentCount).ToList();

                foreach (var deliquencyId in assignmentDeliquencies)
                {
                    newDecisions.Add(new Decision
                    {
                        Id = Guid.NewGuid(),
                        DelinquencyId = deliquencyId,
                        EventDecisionLevelId = model.EventDecisionLevelId,
                        UserId = userAssignment.UserId,
                    }.OnCreateAudit(userId));
                }

                skip += userAssignment.AssigmentCount;
            }

            await this._context.Decision.AddRangeAsync(newDecisions).ConfigureAwait(false);
            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
