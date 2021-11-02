using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Queries
{
    public class GetCountyQuery : BaseQuery<County>, IGetCountyQuery
    {
        private IMapper _mapper;
        private DbSet<County> _entity;
        private ISynergyContext _context;

        public GetCountyQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entity = context.County;
        }

        public int? TotalCount { get; private set; }

        public IGetCountyQuery FilterByState(int stateId)
        {
            andAlsoPredicates.Add(c => c.StateId == stateId);
            return this;
        }

        public IGetCountyQuery FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IGetCountyQuery Take(int take)
        {
            _take = take;
            return this;
        }

        public IGetCountyQuery Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public IGetCountyQuery Search(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return this;
            }

            search = search.Trim().ToLower(CultureInfo.InvariantCulture);
            andAlsoPredicates.Add(x => x.Name.ToLower().StartsWith(search));

            return this;
        }

        public IEnumerable<FastEntityModel<int>> Exeсute()
        {
            var data = BuildQuery().ToList();

            if (_skip != null || _take != null)
            {
                TotalCount = _entity.Where(GetPredicate()).Count();
            }

            return _mapper.Map<IEnumerable<FastEntityModel<int>>>(data);
        }

        public async Task<IEnumerable<FastEntityModel<int>>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = await BuildQuery().ToListAsync(cancellationToken).ConfigureAwait(false);

            if (_skip != null || _take != null)
            {
                TotalCount = await _entity.Where(GetPredicate()).CountAsync(cancellationToken).ConfigureAwait(false);
            }

            return _mapper.Map<IEnumerable<FastEntityModel<int>>>(data);
        }

        private IQueryable<County> BuildQuery()
        {
            IQueryable<County> query =
                _entity
                .Where(GetPredicate())
                .OrderBy(c => c.Name, true)
                .ApplyPaging(_skip, _take);

            return query;
        }
    }
}
