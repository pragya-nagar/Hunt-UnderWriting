using System;
using System.Collections.Generic;
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
    public class GetDataCutResultTypeQuery : BaseQuery<DataCutResultType>, IGetDataCutResultTypeQuery
    {
        private readonly IMapper _mapper;
        private readonly DbSet<DataCutResultType> _entity;

        public GetDataCutResultTypeQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.DataCutResultType;
        }

        public IGetDataCutResultTypeQuery FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FastEntityModel<int>> Exeсute()
        {
            return _mapper.Map<IEnumerable<FastEntityModel<int>>>(_entity.Where(GetPredicate()));
        }

        public async Task<IEnumerable<FastEntityModel<int>>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _mapper.Map<IEnumerable<FastEntityModel<int>>>(await _entity.Where(GetPredicate()).ToListAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
