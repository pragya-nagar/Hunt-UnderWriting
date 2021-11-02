using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models.Assigment;

namespace Synergy.Underwriting.DAL.Queries.Original.Queries
{
    public class GetEventAssigmentsCountQuery : BaseQuery<EventDecisionLevel>, IGetEventAssigmentsCountQuery
    {
        private IMapper _mapper;
        private Microsoft.EntityFrameworkCore.DbSet<EventDecisionLevel> _entity;
        private ISynergyContext _context;
        private Guid _eventId;

        public GetEventAssigmentsCountQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.EventDecisionLevel;
            _context = context;
        }

        public int? TotalCount { get; private set; }

        public IGetEventAssigmentsCountQuery FindById(Guid id)
        {
            _eventId = id;
            andAlsoPredicates.Add(e => e.EventId == id);
            return this;
        }

        public EventAssignmentModel Exeсute()
        {
            var data = new EventAssignmentModel();
            data.Id = _eventId;
            data.DelinquencyAmount = _context.Delinquency.Count(e => e.EventId == _eventId);
            data.AutoProcessedAmount = _context.EventDataCutDecision
                    .Count(ed => ed.EventDataCutStrategy.IsActive && ed.EventDataCutStrategy.EventId == _eventId);

            data.Levels = _mapper.Map<List<EventAssignmentLevelModel>>(_context.EventDecisionLevel.Where(edl => edl.EventId == _eventId));

            var allDecisions = (from d in _context.Decision
                                     where d.EventDecisionLevel.EventId == _eventId && !d.Delinquency.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)
                                     group d by new { d.EventDecisionLevelId, d.UserId } into grp
                                     select new
                                     {
                                         grp.Key.EventDecisionLevelId,
                                         grp.Key.UserId,
                                         DecisionsCount = grp.Count(),
                                     }).ToList();

            var revievedDecisions = (from d in _context.Decision
                                          where d.DecisionTypeId != null && d.EventDecisionLevel.EventId == _eventId && !d.Delinquency.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)
                                          group d by new { d.EventDecisionLevelId, d.UserId } into grp
                                          select new
                                          {
                                              grp.Key.EventDecisionLevelId,
                                              grp.Key.UserId,
                                              RevievedCount = grp.Count(),
                                          }).ToList();

            foreach (EventAssignmentLevelModel level in data.Levels)
            {
                var currentLevelDecisions = allDecisions.Where(l => l.EventDecisionLevelId == level.Id).ToList();
                level.Assignment = new Dictionary<Guid, EventLevelUserAssignmentModel>();
                foreach (var userDecisions in currentLevelDecisions)
                {
                    int processedCount = revievedDecisions
                        .SingleOrDefault(d => d.UserId == userDecisions.UserId && d.EventDecisionLevelId == level.Id)?.RevievedCount ?? 0;

                    level.Assignment.Add(userDecisions.UserId,
                        new EventLevelUserAssignmentModel
                        {
                            Left = userDecisions.DecisionsCount - processedCount,
                            Processed = processedCount,
                        });
                }
            }

            return data;
        }

        public async Task<EventAssignmentModel> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = new EventAssignmentModel();
            data.Id = _eventId;
            data.DelinquencyAmount = _context.Delinquency.Count(e => e.EventId == _eventId);
            data.AutoProcessedAmount = _context.EventDataCutDecision
                    .Count(ed => ed.EventDataCutStrategy.IsActive && ed.EventDataCutStrategy.EventId == _eventId);

            data.Levels = _mapper.Map<List<EventAssignmentLevelModel>>(_context.EventDecisionLevel.Where(edl => edl.EventId == _eventId));

            var allDecisions = await (from d in _context.Decision
                                where d.EventDecisionLevel.EventId == _eventId && !d.Delinquency.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)
                                group d by new { d.EventDecisionLevelId, d.UserId } into grp
                                select new
                                {
                                    grp.Key.EventDecisionLevelId,
                                    grp.Key.UserId,
                                    DecisionsCount = grp.Count(),
                                }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var revievedDecisions = await (from d in _context.Decision
                                     where d.DecisionTypeId != null && d.EventDecisionLevel.EventId == _eventId && !d.Delinquency.EventDataCutDecisions.Any(ed => ed.EventDataCutStrategy.IsActive)
                                     group d by new { d.EventDecisionLevelId, d.UserId } into grp
                                     select new
                                     {
                                         grp.Key.EventDecisionLevelId,
                                         grp.Key.UserId,
                                         RevievedCount = grp.Count(),
                                     }).ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (EventAssignmentLevelModel level in data.Levels)
            {
                var currentLevelDecisions = allDecisions.Where(l => l.EventDecisionLevelId == level.Id).ToList();
                level.Assignment = new Dictionary<Guid, EventLevelUserAssignmentModel>();

                int processedAtLevel = 0;
                foreach (var userDecisions in currentLevelDecisions)
                {
                    int processedCount = revievedDecisions
                        .SingleOrDefault(d => d.UserId == userDecisions.UserId && d.EventDecisionLevelId == level.Id)?.RevievedCount ?? 0;
                    processedAtLevel += processedCount;
                    level.Assignment.Add(userDecisions.UserId,
                        new EventLevelUserAssignmentModel
                        {
                            Left = userDecisions.DecisionsCount - processedCount,
                            Processed = processedCount,
                        });
                }

                level.AvailableRecords = data.DelinquencyAmount - data.AutoProcessedAmount - processedAtLevel;
            }

            return data;
        }
    }
}
