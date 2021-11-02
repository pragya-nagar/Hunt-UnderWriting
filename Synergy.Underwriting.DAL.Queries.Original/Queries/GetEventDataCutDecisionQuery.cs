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
    public class GetEventDataCutDecisionQuery : BaseQuery<EventDataCutDecision>, IGetEventDataCutDecisionQuery
    {
        private IMapper _mapper;
        private DbSet<EventDataCutDecision> _entity;

        public GetEventDataCutDecisionQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.EventDataCutDecision;
        }

        public IGetEventDataCutDecisionQuery FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IGetEventDataCutDecisionQuery FindByEventId(Guid eventId)
        {
            andAlsoPredicates.Add(e => e.EventDataCutStrategy.EventId == eventId);
            return this;
        }

        public IGetEventDataCutDecisionQuery FindByIsActive(bool isActive)
        {
            andAlsoPredicates.Add(e => e.EventDataCutStrategy.IsActive);
            return this;
        }

        public IEnumerable<EventDataCutDecisionModel> Exeсute()
        {
            var data = BuildQuery();
            return _mapper.Map<IEnumerable<EventDataCutDecisionModel>>(data.ToList());
        }

        public async Task<IEnumerable<EventDataCutDecisionModel>> ExeсuteAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = BuildQuery();
            return _mapper.Map<IEnumerable<EventDataCutDecisionModel>>(await data.ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        private IQueryable<EventDataCutDecision> BuildQuery()
        {
            includes.Add(x => x.EventDataCutStrategy);
            IQueryable<EventDataCutDecision> query = _entity
                .IncludeMultiple(includes.ToArray())
                .Where(GetPredicate());

            return query;
        }
    }
}
