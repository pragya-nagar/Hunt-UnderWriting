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
    public class GetDataCutRuleFieldsQuery : BaseQuery<DataCutRuleField>, IGetDataCutRuleFieldsQuery
    {
        private readonly IMapper _mapper;
        private readonly DbSet<DataCutRuleField> _entity;

        public GetDataCutRuleFieldsQuery(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._entity = context.DataCutRuleField;
        }

        public int? TotalCount { get; private set; }

        public IGetDataCutRuleFieldsQuery FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DataCutRuleFieldModel> Exeсute()
        {
            return _mapper.Map<IEnumerable<DataCutRuleFieldModel>>(BuildQuery());
        }

        public async Task<IEnumerable<DataCutRuleFieldModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<DataCutRuleField> data = BuildQuery();
            return _mapper.Map<IEnumerable<DataCutRuleFieldModel>>(await data.ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        private IQueryable<DataCutRuleField> BuildQuery()
        {
            includes.Add(e => e.DataCutFieldType.DataCutLogicTypes);
            IQueryable<DataCutRuleField> query = _entity
                .IncludeMultiple(includes.ToArray())
                .Where(GetPredicate());

            return query;
        }
    }
}
