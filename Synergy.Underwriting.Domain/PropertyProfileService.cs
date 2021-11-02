using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.Exceptions;
using Synergy.DataAccess.Abstractions;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.PropertyProfile;

namespace Synergy.Underwriting.Domain
{
    public class PropertyProfileService : IPropertyProfileService
    {
        private readonly IMapper _mapper;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyProfileRule> _ruleQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyProfile> _propertyProfileQueryProvider;

        public PropertyProfileService(IMapper mapper,
            IQueryProvider<DAL.Queries.Entities.PropertyProfileRule> ruleQueryProvider,
            IQueryProvider<DAL.Queries.Entities.PropertyProfile> propertyProfileQueryProvider)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._ruleQueryProvider = ruleQueryProvider ?? throw new ArgumentNullException(nameof(ruleQueryProvider));
            this._propertyProfileQueryProvider = propertyProfileQueryProvider ?? throw new ArgumentNullException(nameof(propertyProfileQueryProvider));
        }

        public async Task<SearchResultModel<PropertyProfileRuleModel>> GetRulesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<DAL.Queries.Entities.PropertyProfileRule> query = this._ruleQueryProvider.Query
                .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileRuleItemValues)
                .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileRuleField)
                .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileLogicType)
                .Where(x => x.DeletedOn == null);

            var items = this._mapper.Map<List<PropertyProfileRuleModel>>(await query.ToListAsync(cancellationToken).ConfigureAwait(false));

            return new SearchResultModel<PropertyProfileRuleModel>()
            {
                List = items,
                TotalCount = items.Count,
            };
        }

        public async Task<SearchResultModel<PropertyProfileModel>> GetListAsync(SearchArgsModel<PropertyProfileFilterArgs, PropertyProfileSortField> args, CancellationToken cancellationToken)
        {
            IQueryable<DAL.Queries.Entities.PropertyProfile> query = this._propertyProfileQueryProvider.Query
                .Include(x => x.PropertyProfileStates)
                .ThenInclude(x => x.State).Where(x => x.DeletedOn == null);

            IEnumerable<PropertyProfileModel> items;
            int count;

            if (string.IsNullOrWhiteSpace(args?.FullSearch) == false)
            {
                string val = args.FullSearch.Trim().ToLower(CultureInfo.InvariantCulture);
                query = query.Where(x => x.Name.ToLower().Contains(val)
                                      || x.PropertyProfileStates.Any(p => p.State.Abbreviation.ToLower().StartsWith(val)));
            }

            if (args?.Filter?.IsActive.HasValue == true)
            {
                query = query.Where(x => x.IsActive == args.Filter.IsActive);
            }

            if (args?.Filter?.StateId.HasValue == true)
            {
                query = query.Where(x => x.PropertyProfileStates.Any(p => p.StateId == args.Filter.StateId));
            }

            count = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            if (args?.SortField != null)
            {
                switch (args.SortField)
                {
                    case PropertyProfileSortField.Name:
                        query = (args.SortOrder ?? SortOrder.Asc) == SortOrder.Asc ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                        break;
                    case PropertyProfileSortField.Status:
                        query = (args.SortOrder ?? SortOrder.Asc) == SortOrder.Asc ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive);
                        break;
                }
            }

            int skip = args?.Offset ?? 0;
            int take = args?.Limit ?? 50;
            query = query.ApplyPaging(skip, take);

            items = this._mapper.Map<List<PropertyProfileModel>>(await query.ToListAsync(cancellationToken).ConfigureAwait(false));

            return new SearchResultModel<PropertyProfileModel>()
            {
                TotalCount = count,
                List = items,
            };
        }

        public async Task<PropertyProfileDetailsModel> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<DAL.Queries.Entities.PropertyProfile> query = this._propertyProfileQueryProvider.Query
               .Include(x => x.PropertyProfileStates).ThenInclude(x => x.State)
               .Include(x => x.PropertyProfileRulePropertyProfiles)
               .Where(x => x.Id == id && x.DeletedOn == null);

            var item = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (item == null)
            {
                throw new NotFoundException();
            }

            IQueryable<DAL.Queries.Entities.PropertyProfileRule> rulesQuery = this._ruleQueryProvider.Query
                .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileLogicType)
                .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileRuleField)
                .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileRuleItemValues)
                .Where(x => item.PropertyProfileRulePropertyProfiles.Any(r => r.PropertyProfileRuleId == x.Id) && x.DeletedOn == null);

            var propertyProfile = this._mapper.Map<PropertyProfileDetailsModel>(item);
            propertyProfile.PropertyProfileRules = this._mapper.Map<List<PropertyProfileRuleModel>>(rulesQuery);

            return propertyProfile;
        }

        public async Task<PropertyProfileRuleModel> FindRuleAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<DAL.Queries.Entities.PropertyProfileRule> rulesQuery = this._ruleQueryProvider.Query
                         .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileLogicType)
                         .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileRuleField)
                         .Include(x => x.PropertyProfileRuleItems).ThenInclude(x => x.PropertyProfileRuleItemValues)
                         .Where(x => x.Id == id && x.DeletedOn == null);

            var item = await rulesQuery.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var rule = item == null ? throw new NotFoundException() : this._mapper.Map<PropertyProfileRuleModel>(item);

            return rule;
        }
    }
}
