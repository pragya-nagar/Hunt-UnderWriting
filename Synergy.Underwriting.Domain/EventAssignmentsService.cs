using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.Exceptions;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Commands.EventAssignment;
using Synergy.Underwriting.Models.PropertyProfile;

namespace Synergy.Underwriting.Domain
{
    public class EventAssignmentsService : IEventAssignmentsService
    {
        private readonly IMapper _mapper;
        private readonly IPublishMessage _publisher;
        private readonly IQueryProvider<DAL.Queries.Entities.Delinquency> _delinquencyQuery;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyProfile> _propertyProfileQuery;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyProfileDelinquency> _propertyProfileDelinquencyQuery;
        private readonly IQueryProvider<DAL.Queries.Entities.EventDecisionLevel> _eventDecisionLevelQuery;
        private readonly IQueryProvider<DAL.Queries.Entities.Event> _eventQuery;
        private readonly IQueryProvider<DAL.Queries.Entities.EventDecisionLevelPropertyProfile> _eventDecisionLevelPropertyProfileQuery;

        public EventAssignmentsService(IMapper mapper,
                                       IPublishMessage publisher,
                                       IQueryProvider<DAL.Queries.Entities.Delinquency> delinquencyQuery,
                                       IQueryProvider<DAL.Queries.Entities.PropertyProfile> propertyProfileQuery,
                                       IQueryProvider<DAL.Queries.Entities.PropertyProfileDelinquency> propertyProfileDelinquencyQuery,
                                       IQueryProvider<DAL.Queries.Entities.EventDecisionLevel> eventDecisionLevelUserQuery,
                                       IQueryProvider<DAL.Queries.Entities.Event> eventQuery,
                                       IQueryProvider<DAL.Queries.Entities.EventDecisionLevelPropertyProfile> eventDecisionLevelPropertyProfileQuery)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this._eventQuery = eventQuery ?? throw new ArgumentNullException(nameof(eventQuery));
            this._delinquencyQuery = delinquencyQuery ?? throw new ArgumentNullException(nameof(delinquencyQuery));
            this._propertyProfileQuery = propertyProfileQuery ?? throw new ArgumentNullException(nameof(propertyProfileQuery));
            this._propertyProfileDelinquencyQuery = propertyProfileDelinquencyQuery ?? throw new ArgumentNullException(nameof(propertyProfileDelinquencyQuery));
            this._eventDecisionLevelQuery = eventDecisionLevelUserQuery ?? throw new ArgumentNullException(nameof(eventDecisionLevelUserQuery));
            this._eventDecisionLevelPropertyProfileQuery = eventDecisionLevelPropertyProfileQuery ?? throw new ArgumentNullException(nameof(eventDecisionLevelPropertyProfileQuery));
        }

        public async Task<EventAssignmentProfileModel> FindAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            // Get`s event state Id
            int stateId = await _eventQuery.Query.Where(x => x.Id == eventId).Select(x => x.StateId).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (stateId == 0)
            {
                throw new NotFoundException();
            }

            // Filter delinquency by event
            IQueryable<DAL.Queries.Entities.Delinquency> delinquencyQuery = this._delinquencyQuery.Query.Where(d => d.EventId == eventId);

            // Get`s total delinquency count
            int totalCount = await delinquencyQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            if (totalCount == 0)
            {
                return new EventAssignmentProfileModel();
            }

            // Get`s delinquency without profiles count
            var otherCount = await delinquencyQuery.Where(d =>

                // Exclude delinquencies with data cut decession
                !d.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)

                // Excludes delinquencies that are assigned to active profile or delinquencies that was already assigned to currently inactive profile
                && !d.PropertyProfileDelinquencies.Any(ppd =>
                    (ppd.PropertyProfile.PropertyProfileStates.Any(pps => pps.StateId == stateId) && ppd.PropertyProfile.IsActive == true)
                    || ppd.PropertyProfile.EventDecisionLevelPropertyProfile.Any(l => l.EventDecisionLevel.EventId == eventId)))
            .CountAsync(cancellationToken).ConfigureAwait(false);

            // Get`s delinquencies count with auto decision
            var autoProcessedCount = await this._delinquencyQuery.Query.Where(d => d.EventId == eventId && d.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)).CountAsync(cancellationToken).ConfigureAwait(false);

            var propertyProfiles = await _propertyProfileQuery.Query.Where(pp =>

                                                            // Get`s property profiles that are active or was used at current event
                                                            (pp.PropertyProfileStates.Any(x => x.StateId == stateId) && pp.IsActive == true)
                                                            || pp.EventDecisionLevelPropertyProfile.Any(l => l.EventDecisionLevel.EventId == eventId))
                                                             .Select(x => new AssignmentPropertyProfileModel
                                                             {
                                                                 Id = x.Id,
                                                                 Name = x.Name,
                                                                 PropertyCount = x.PropertyProfileDelinquencies.Where(ppd => ppd.Delinquency.EventId == eventId
                                                                                 && !ppd.Delinquency.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)).Count(),
                                                             }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var eventAssignmentProfileModel = new EventAssignmentProfileModel
            {
                TotalPropertyCount = totalCount,
                OtherPropertyCount = otherCount,
                AutoProcessedCount = autoProcessedCount,
                PropertyProfiles = propertyProfiles,
            };

            return eventAssignmentProfileModel;
        }

        public async Task<EventAssignmentProfileModel> GetOrderedProfilesAsync(OrderedProfileArgs args, CancellationToken cancellationToken = default)
        {
            var delinquencyQuery = this._delinquencyQuery.Query.Where(d => d.EventId == args.EventId && !d.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive));

            var totalCount = await delinquencyQuery.CountAsync(cancellationToken).ConfigureAwait(false);
            if (totalCount == 0)
            {
                return new EventAssignmentProfileModel();
            }

            var propertyProfiles = await _propertyProfileQuery.Query.Where(pp => pp.IsActive == true && args.ProfileOrders.Any(x => x.PropertyProfileId == pp.Id))
                                                                    .Select(x => new AssignmentPropertyProfileModel
                                                                    {
                                                                        Id = x.Id,
                                                                        Name = x.Name,
                                                                    }).ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var propertyProfile in propertyProfiles)
            {
                PropertyProfileOrderModel currentProfile = args.ProfileOrders.Single(x => x.PropertyProfileId == propertyProfile.Id);
                List<Guid> previouseOrdersProfiles = args.ProfileOrders.Where(x => x.Order < currentProfile.Order).Select(d => d.PropertyProfileId).ToList();

                propertyProfile.PropertyCount = await this._propertyProfileDelinquencyQuery.Query.Where(
                                    cur =>
                                        cur.PropertyProfileId == propertyProfile.Id
                                        && cur.Delinquency.EventId == args.EventId
                                        && !cur.Delinquency.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)
                                        && !this._propertyProfileDelinquencyQuery.Query.Any(
                                                    prev => previouseOrdersProfiles.Contains(prev.PropertyProfileId)
                                                    && prev.DelinquencyId == cur.DelinquencyId)).CountAsync(cancellationToken).ConfigureAwait(false);
            }

            var propertyCount = propertyProfiles.Sum(x => x.PropertyCount);

            var eventAssignmentProfileModel = new EventAssignmentProfileModel
            {
                TotalPropertyCount = totalCount,
                OtherPropertyCount = totalCount - propertyCount,
                PropertyProfiles = propertyProfiles,
            };

            return eventAssignmentProfileModel;
        }

        public async Task<EventAssignmentModel> GetAssignmentsAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var delinquencyQuery = this._delinquencyQuery.Query.Where(d => d.EventId == eventId);

            var totalCount = await delinquencyQuery.CountAsync(cancellationToken).ConfigureAwait(false);
            if (totalCount == 0)
            {
                return new EventAssignmentModel();
            }

            var autoProcessedIds = await this._delinquencyQuery.Query.Where(d => d.EventId == eventId && d.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)).Select(d => d.Id).ToListAsync(cancellationToken).ConfigureAwait(false);

            List<LevelAssignments> levelAssignments = await this._eventDecisionLevelQuery.Query.Where(edl => edl.EventId == eventId)
                                        .Select(edl => new LevelAssignments
                                        {
                                            Id = edl.Id,
                                            Name = edl.Name,
                                            Order = edl.Order,
                                            Assignments = edl.EventDecisionLevelUser.Select(edlu =>
                                            new Assignments()
                                            {
                                                ProfileOrder = edlu.EventDecisionLevelPropertyProfileId.HasValue ? edlu.EventDecisionLevelPropertyProfile.Order : (int?)null,
                                                PropertyProfileId = edlu.EventDecisionLevelPropertyProfileId.HasValue ? edlu.EventDecisionLevelPropertyProfile.PropertyProfileId : (Guid?)null,
                                                UsersAssignment = new UsersAssignment
                                                {
                                                    UserId = edlu.UserId,
                                                    AssignmentsCount = edl.Decisions.Count(
                                                        d => d.UserId == edlu.UserId
                                                            &&
                                                            (
                                                                (!edlu.EventDecisionLevelPropertyProfileId.HasValue && !d.PropertyProfileId.HasValue)
                                                                ||
                                                                (edlu.EventDecisionLevelPropertyProfileId.HasValue && d.PropertyProfileId.HasValue && d.PropertyProfileId == edlu.EventDecisionLevelPropertyProfile.PropertyProfileId))
                                                            && !d.Delinquency.EventDataCutDecisions.Any(edcd => edcd.EventDataCutStrategy.IsActive)),
                                                    ProcessedAssignmentCount = edl.Decisions.Count(
                                                            d => d.UserId == edlu.UserId
                                                            && d.DecisionTypeId != null
                                                            &&
                                                            (
                                                                (!edlu.EventDecisionLevelPropertyProfileId.HasValue && !d.PropertyProfileId.HasValue)
                                                                ||
                                                                (edlu.EventDecisionLevelPropertyProfileId.HasValue && d.PropertyProfileId.HasValue && d.PropertyProfileId == edlu.EventDecisionLevelPropertyProfile.PropertyProfileId))
                                                            && !d.Delinquency.EventDataCutDecisions.Any(edcd => edcd.EventDataCutStrategy.IsActive)),
                                                },
                                            }).ToList(),
                                        }).ToListAsync(cancellationToken).ConfigureAwait(false);

            List<AssignmentPropertyProfileModel> propertyProfiles = new List<AssignmentPropertyProfileModel>();

            foreach (var levelItem in levelAssignments.OrderBy(x => x.Order))
            {
                Guid[] profileIds = levelItem.Assignments.Where(x => x.PropertyProfileId != null).Select(x => x.PropertyProfileId.Value).ToArray();

                var assignments = await this._eventDecisionLevelPropertyProfileQuery.Query
                    .Where(x => x.EventDecisionLevelId == levelItem.Id &&
                                profileIds.Contains(x.PropertyProfileId) == false &&
                                x.DeletedOn == null).ToListAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in assignments)
                {
                    levelItem.Assignments.Add(new Assignments
                    {
                        PropertyProfileId = item.PropertyProfileId,
                        ProfileOrder = item.Order,
                        UsersAssignment = new UsersAssignment(),
                    });
                }

                var profiles = levelItem.Assignments.Where(pp => pp.PropertyProfileId.HasValue == true).Select(x => new { x.PropertyProfileId, x.ProfileOrder }).Distinct().ToList();

                foreach (var currentProfile in profiles)
                {
                    var previouseOrdersProfiles = levelItem.Assignments.Where(x => x.ProfileOrder < currentProfile.ProfileOrder).Select(d => d.PropertyProfileId).ToList();

                    propertyProfiles.Add(new AssignmentPropertyProfileModel
                    {
                        Id = currentProfile.PropertyProfileId.Value,
                        PropertyCount = await this._propertyProfileDelinquencyQuery.Query.Where(
                                        cur =>
                                            cur.PropertyProfileId == currentProfile.PropertyProfileId
                                            && cur.Delinquency.EventId == eventId
                                            && !cur.Delinquency.EventDataCutDecisions.Any(
                                                ed => ed.EventDataCutStrategy.IsActive)
                                            && !this._propertyProfileDelinquencyQuery.Query.Any(
                                                prev => previouseOrdersProfiles.Contains(prev.PropertyProfileId)
                                                        && prev.DelinquencyId == cur.DelinquencyId))
                                    .CountAsync(cancellationToken)
                                    .ConfigureAwait(false),
                    });
                }
            }

            var eventAssignment = new EventAssignmentModel
            {
                AutoProcessedCount = autoProcessedIds.Count,
                TotalPropertyCount = totalCount,
                Levels = levelAssignments.Select(level => new LevelModel
                {
                    Id = level.Id,
                    Name = level.Name,
                    Order = level.Order,
                    Assignments = level.Assignments.GroupBy(g => g.PropertyProfileId)
                                                   .Select(a => new LevelPropertyProfileAssignmensModel
                                                   {
                                                       ProfileOrder = a.FirstOrDefault()?.ProfileOrder,
                                                       PropertyProfileId = a.Key.HasValue ? a.Key : null,
                                                       PropertyCount = a.Key.HasValue ? propertyProfiles.Where(x => x.Id == a.Key).First().PropertyCount : 0,
                                                       UsersAssignment = a.Select(ua => new PropertyProfileUserAssignmentModel
                                                       {
                                                           AssignmentsCount = ua.UsersAssignment.AssignmentsCount,
                                                           UserId = ua.UsersAssignment.UserId,
                                                           ProcessedAssignmentCount = ua.UsersAssignment.ProcessedAssignmentCount,
                                                       }),
                                                   }),
                }).ToList(),
            };

            eventAssignment.Levels.ForEach(x => x.OtherPropertyCount = totalCount - x.Assignments.Sum(s => s.PropertyCount));

            return eventAssignment;
        }

        public async Task CreateAssignmentAsync(EventAssignmentCreateCommand command, CancellationToken cancellationToken = default)
        {
            var @event = await this._eventQuery.Query.Include(s => s.State).Where(x => x.Id == command.EventId && x.DeletedOn == null)
                                                                                 .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (@event.IsFreezed)
            {
                throw new NotAcceptableException($"Property Profiles for state {@event.State.Name} are currently updating. Please wait until all changes will be saved in order to continue with assignment");
            }

            if (@event.IsLocked)
            {
                throw new NotAcceptableException($"Event with Id {@event.Id} is locked. You can't perform assignment or reassignment operation");
            }

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAssignmentAsync(EventAssignmentUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var @event = await this._eventQuery.Query.Include(s => s.State).Where(x => x.Id == command.EventId && x.DeletedOn == null)
                                                                                 .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (@event.IsFreezed)
            {
                throw new NotAcceptableException($"Property Profiles for state {@event.State.Name} are currently updating. Please wait until all changes will be saved in order to continue with assignment");
            }

            if (@event.IsLocked)
            {
                throw new NotAcceptableException($"Event with Id {@event.Id} is locked. You can't perform assignment or reassignment operation");
            }

            await this._publisher.PublishAsync(command, cancellationToken).ConfigureAwait(false);
        }

        private class LevelAssignments
        {
            internal Guid Id { get; set; }

            internal string Name { get; set; }

            internal int Order { get; set; }

            internal List<Assignments> Assignments { get; set; }
        }

        private class Assignments
        {
            internal int? ProfileOrder { get; set; }

            internal Guid? PropertyProfileId { get; set; }

            internal UsersAssignment UsersAssignment { get; set; }
        }

        private class UsersAssignment
        {
            internal Guid UserId { get; set; }

            internal int AssignmentsCount { get; set; }

            internal int ProcessedAssignmentCount { get; set; }
        }
    }
}
