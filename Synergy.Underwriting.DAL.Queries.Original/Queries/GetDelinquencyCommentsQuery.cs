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
    public class GetDelinquencyCommentsQuery : BaseQuery<DelinquencyComment>, IGetDelinquencyCommentsQuery
    {
        private IMapper _mapper;
        private DbSet<DelinquencyComment> _entity;

        public GetDelinquencyCommentsQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.DelinquencyComment;
        }

        public int? TotalCount { get; private set; }

        public IGetDelinquencyCommentsQuery FindById(Guid id)
        {
            andAlsoPredicates.Add(lc => lc.Id == id);
            return this;
        }

        public IGetDelinquencyCommentsQuery FilterByDelinquencies(IEnumerable<Guid> ids)
        {
            andAlsoPredicates.Add(lc => ids.Contains(lc.DelinquencyId));
            return this;
        }

        public IGetDelinquencyCommentsQuery Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public IGetDelinquencyCommentsQuery Take(int take)
        {
            _take = take;
            return this;
        }

        public IEnumerable<DelinquencyCommentModel> Exeсute()
        {
            IQueryable<DelinquencyComment> data = BuildQuery();

            if (_skip != null || _take != null)
            {
                TotalCount = _entity.Where(GetPredicate()).Count();
            }

            return _mapper.Map<IEnumerable<DelinquencyCommentModel>>(data.ToList());
        }

        public async Task<IEnumerable<DelinquencyCommentModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<DelinquencyComment> data = BuildQuery();

            if (_skip != null || _take != null)
            {
                TotalCount = await _entity.Where(GetPredicate()).CountAsync(cancellationToken).ConfigureAwait(false);
            }

            return _mapper.Map<IEnumerable<DelinquencyCommentModel>>(await data.ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        private IQueryable<DelinquencyComment> BuildQuery()
        {
            _sortSelector = e => e.CommentDate;
            includes.Add(c => c.Author);
            IQueryable<DelinquencyComment> query = _entity.IncludeMultiple(includes.ToArray())
                .Where(GetPredicate())
                .OrderBy(_sortSelector, false)
                .ApplyPaging(_skip, _take);

            return query;
        }
    }
}
