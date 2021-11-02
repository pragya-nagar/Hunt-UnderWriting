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
    public class GetStateTaxRatesQuery : BaseQuery<StateTaxe>, IGetStateTaxRatesQuery
    {
        private IMapper _mapper;
        private DbSet<StateTaxe> _entity;

        public GetStateTaxRatesQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.StateTaxe;
        }

        public IGetStateTaxRatesQuery FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IGetStateTaxRatesQuery FindByStateId(int stateId)
        {
            andAlsoPredicates.Add(s => s.StateId == stateId);
            return this;
        }

        public IGetStateTaxRatesQuery IncludeAudit()
        {
            includes.Add(p => p.CreatedBy);
            includes.Add(p => p.ModifiedBy);
            return this;
        }

        public IEnumerable<StateTaxRateModel> Exeсute()
        {
            List<StateTaxe> data = BuildQuery().ToList();
            return _mapper.Map<List<StateTaxRateModel>>(data);
        }

        public async Task<IEnumerable<StateTaxRateModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            List<StateTaxe> data = await BuildQuery().ToListAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<List<StateTaxRateModel>>(data);
        }

        private IQueryable<StateTaxe> BuildQuery()
        {
            includes.Add(p => p.State);
            var query = _entity
                .IncludeMultiple(includes.ToArray())
                .Where(GetPredicate());

            return query;
        }
    }
}
