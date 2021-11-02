using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Underwriting.Models.Commands.EventAssignment;
using Synergy.Underwriting.Models.PropertyProfile;

namespace Synergy.Underwriting.Domain.Abstracts
{
    public interface IEventAssignmentsService
    {
        Task<EventAssignmentProfileModel> FindAsync(Guid eventId, CancellationToken cancellationToken = default);

        Task<EventAssignmentProfileModel> GetOrderedProfilesAsync(OrderedProfileArgs args, CancellationToken cancellationToken = default);

        Task<EventAssignmentModel> GetAssignmentsAsync(Guid eventId, CancellationToken cancellationToken = default);

        Task CreateAssignmentAsync(EventAssignmentCreateCommand command, CancellationToken cancellationToken = default);

        Task UpdateAssignmentAsync(EventAssignmentUpdateCommand command, CancellationToken cancellationToken = default);
    }
}
