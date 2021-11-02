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
    public class GetDataCutLogicTypesQuery : BaseQuery<DataCutLogicType>, IGetDataCutLogicTypesQuery
    {
        private readonly IMapper _mapper;
        private readonly DbSet<DataCutLogicType> _entity;

        public GetDataCutLogicTypesQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.DataCutLogicType;
        }

        public IGetDataCutLogicTypesQuery FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DataCutLogicTypeModel> Exeсute()
        {
            return _mapper.Map<IEnumerable<DataCutLogicTypeModel>>(_entity.Where(GetPredicate()));
        }

        public async Task<IEnumerable<DataCutLogicTypeModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _mapper.Map<IEnumerable<DataCutLogicTypeModel>>(await _entity.Where(GetPredicate()).ToListAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
