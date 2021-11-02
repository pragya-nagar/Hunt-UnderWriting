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
    public class CreateProfileAssignmentCommand : ICreateProfileAssignmentCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreateProfileAssignmentCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(CreateProfileAssignmentModel model, Guid userId)
        {
            this.DispatchAsync(model, userId).Wait();
        }

        public async Task<int> DispatchAsync(CreateProfileAssignmentModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Guid> profileDeliquenciesIds = await this._context.PropertyProfileDelinquency
                .Where(x => x.Delinquency.EventId == model.EventId
                            && !x.Delinquency.EventDataCutDecisions.Any(d => d.EventDataCutStrategy.IsActive == true)
                            && x.PropertyProfileId == model.ProfileId
                            && model.PreviousProfiles.Contains(x.PropertyProfileId) == false
                            && x.DeletedOn == null)
                .Select(x => x.DelinquencyId).ToListAsync(cancellationToken).ConfigureAwait(false);

            List<Guid> decisionIds = await this._context.Decision
                .Where(x => x.EventDecisionLevelId == model.EventDecisionLevelId && x.DeletedOn == null)
                .Select(x => x.DelinquencyId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            // Deliquencies with DecisionTypeId will be excluded
            List<Guid> deliquenciesIds = profileDeliquenciesIds.Where(x => decisionIds.Contains(x) == false).ToList();

            List<Decision> newDecisions = new List<Decision>();

            if (deliquenciesIds.Any() == false)
            {
                return 0;
            }

            int skip = 0;

            foreach (var userAssignment in model.UserAssignment)
            {
                List<Guid> assignmentList = deliquenciesIds.Skip(skip).Take(userAssignment.AssignmentsCount).ToList(); // skip
                if (assignmentList.Any() == false)
                {
                    break;
                }

                foreach (var item in assignmentList)
                {
                    newDecisions.Add(new Decision
                    {
                        Id = Guid.NewGuid(),
                        DelinquencyId = item,
                        EventDecisionLevelId = model.EventDecisionLevelId,
                        UserId = userAssignment.UserId,
                        PropertyProfileId = model.ProfileId,
                    }.OnCreateAudit(userId));
                }

                skip += userAssignment.AssignmentsCount;
            }

            await this._context.Decision.AddRangeAsync(newDecisions, cancellationToken).ConfigureAwait(false);
            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
