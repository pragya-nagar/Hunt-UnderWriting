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
    public class GetDelinquencyQuery : BaseQuery<Delinquency>, IGetDelinquencyQuery
    {
        private IMapper _mapper;
        private DbSet<Delinquency> _entity;

        public GetDelinquencyQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.Delinquency;
        }

        public int? TotalCount { get; private set; }

        public IGetDelinquencyQuery FindById(Guid id)
        {
            andAlsoPredicates.Add(u => u.Id == id);
            return this;
        }

        public IGetDelinquencyQuery Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public IGetDelinquencyQuery Take(int take)
        {
            _take = take;
            return this;
        }

        public IGetDelinquencyQuery FilterByPropertyIds(List<Guid> propertyIds)
        {
            andAlsoPredicates.Add(d => propertyIds.Contains(d.PropertyId));
            return this;
        }

        public IGetDelinquencyQuery FilterByEventIds(List<Guid> eventIds)
        {
            andAlsoPredicates.Add(d => d.EventId != null && eventIds.Contains((Guid)d.EventId));
            return this;
        }

        public IEnumerable<DelinquencyModel> Exeсute()
        {
            IEnumerable<Delinquency> data = BuildQuery();

            if (_skip != null || _take != null)
            {
                TotalCount = _entity.Where(GetPredicate()).Count();
            }

            return _mapper.Map<IEnumerable<DelinquencyModel>>(data);
        }

        public async Task<IEnumerable<DelinquencyModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<Delinquency> data = BuildQuery();

            if (_skip != null || _take != null)
            {
                TotalCount = await _entity.Where(GetPredicate()).CountAsync().ConfigureAwait(false);
            }

            return _mapper.Map<IEnumerable<DelinquencyModel>>(await data.ToListAsync().ConfigureAwait(false));
        }

        private IQueryable<Delinquency> BuildQuery()
        {
            IQueryable<Delinquency> query = _entity
                .IncludeMultiple(includes.ToArray())
                .Where(GetPredicate())
                .ApplyPaging(_skip, _take);

            return query;
        }
    }
}
