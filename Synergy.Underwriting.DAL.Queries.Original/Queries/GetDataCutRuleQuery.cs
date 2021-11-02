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
    public class GetDataCutRuleQuery : BaseQuery<DataCutRule>, IGetDataCutRuleQuery
    {
        private readonly IMapper _mapper;
        private readonly DbSet<DataCutRule> _entity;

        public GetDataCutRuleQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.DataCutRule;
        }

        public IGetDataCutRuleQuery FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IGetDataCutRuleQuery FindByCounty(int countyId)
        {
            andAlsoPredicates.Add(r => r.CountyId == countyId);
            return this;
        }

        public IEnumerable<DataCutRuleModel> Exeсute()
        {
            return _mapper.Map<IEnumerable<DataCutRuleModel>>(BuildQuery());
        }

        public async Task<IEnumerable<DataCutRuleModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _mapper.Map<IEnumerable<DataCutRuleModel>>(await BuildQuery().ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        private IQueryable<DataCutRule> BuildQuery()
        {
            includes.Add(r => r.DataCutResultType);

            IQueryable<DataCutRule> query = _entity.Include(e => e.DataCutRuleItems).ThenInclude(dl => dl.DataCutLogicType)
                .Include(e => e.DataCutRuleItems).ThenInclude(dl => dl.DataCutRuleField)
                .IncludeMultiple(includes.ToArray())
                .Where(GetPredicate());

            return query;
        }
    }
}
