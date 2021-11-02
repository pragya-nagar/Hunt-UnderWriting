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
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Queries
{
    public class GetEventDecisionLevelQuery : BaseQuery<EventDecisionLevel>, IGetEventDecisionLevelQuery
    {
        private IMapper _mapper;
        private DbSet<EventDecisionLevel> _entity;
        private ISynergyContext _context;

        public GetEventDecisionLevelQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.EventDecisionLevel;
            _context = context;
        }

        public int? TotalCount { get; private set; }

        public IGetEventDecisionLevelQuery FilterByEventId(Guid eventId)
        {
            andAlsoPredicates.Add(e => e.EventId == eventId);
            return this;
        }

        public IGetEventDecisionLevelQuery FindById(Guid id)
        {
            andAlsoPredicates.Add(d => d.Id == id);
            return this;
        }

        public IGetEventDecisionLevelQuery Take(int take)
        {
            _take = take;
            return this;
        }

        public IGetEventDecisionLevelQuery Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public IEnumerable<EventDecisionLevelModel> Exeсute()
        {
            var data = GetEventDecisionLevels().ToList();

            if (_skip != null || _take != null)
            {
                TotalCount = _entity.Where(GetPredicate()).Count();
            }

            return _mapper.Map<IEnumerable<EventDecisionLevelModel>>(data);
        }

        public async Task<IEnumerable<EventDecisionLevelModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = await GetEventDecisionLevels().ToListAsync(cancellationToken).ConfigureAwait(false);

            if (_skip != null || _take != null)
            {
                TotalCount = await _entity.Where(GetPredicate()).CountAsync(cancellationToken).ConfigureAwait(false);
            }

            return _mapper.Map<IEnumerable<EventDecisionLevelModel>>(data);
        }

        private IQueryable<EventDecisionLevel> GetEventDecisionLevels()
        {
            IQueryable<EventDecisionLevel> query =
                _entity
                .Where(GetPredicate())
                .OrderBy(x => x.Order, true)
                .ApplyPaging(_skip, _take);

            return query;
        }
    }
}
