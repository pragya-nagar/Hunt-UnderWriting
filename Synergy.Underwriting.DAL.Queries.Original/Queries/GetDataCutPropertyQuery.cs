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
    public class GetDataCutPropertyQuery : BaseQuery<Delinquency>, IGetDataCutPropertyQuery
    {
        private IMapper _mapper;
        private DbSet<Delinquency> _entity;
        private ISynergyContext _context;

        public GetDataCutPropertyQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.Delinquency;
            _context = context;
        }

        public IGetDataCutPropertyQuery FindById(Guid id)
        {
            andAlsoPredicates.Add(d => d.EventId == id);
            return this;
        }

        public IEnumerable<DataCutPropertyModel> Exeсute()
        {
            List<Delinquency> data = BuidQuery().ToList();
            if (!data.Any())
            {
                return null;
            }

            Guid eventId = (Guid)data.First().EventId;

            var results = _mapper.Map<List<DataCutPropertyModel>>(data);
            var propertyIds = results.Select(r => r.PropertyId).ToList();
            var propertyValuations = GetPropertyValuations(propertyIds).ToList();

            foreach (var result in results)
            {
                var propertyValuation = propertyValuations.FirstOrDefault(pv => pv.PropertyId == result.PropertyId);
                result.ImprovementValue = propertyValuation?.ImprovementValue;
                result.AppraisedValue = propertyValuation?.AppraisedValue;
                result.LandValue = propertyValuation?.LandValue;
            }

            return results;
        }

        public Task<IEnumerable<DataCutPropertyModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        private IQueryable<Delinquency> BuidQuery()
        {
            var query = _entity
                .Include(d => d.Property).ThenInclude(p => p.Lead)
                .Include(d => d.Property).ThenInclude(p => p.InternalLandUseCode)
                .Include(d => d.Property).ThenInclude(p => p.GeneralLandUseCode)
                .Include(d => d.PropertySupplementalEventData)
                .Where(GetPredicate());

            return query;
        }

        private IQueryable<PropertyValuation> GetPropertyValuations(List<Guid> propertyIds)
        {
            var query = _context.PropertyValuation.Where(pv => propertyIds.Contains(pv.PropertyId) && pv.IsActive);

            return query;
        }
    }
}
