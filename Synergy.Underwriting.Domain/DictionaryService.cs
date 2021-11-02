using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.Domain.Models.Common;
using Synergy.DataAccess.Dictionaries.Queries.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.PropertyProfile;
using Synergy.Underwriting.Models.Rule;

namespace Synergy.Underwriting.Domain
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IMapper _mapper;

        private readonly IQueryProvider<DAL.Queries.Entities.Event> _eventQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.GeneralLandUseCode> _generalLandUseCodeQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.InternalLandUseCode> _internalLandUseCodeQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyProfileRuleField> _propertyProfileRuleFieldQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DepartmentRole> _departmentRolesQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.UserRole> _userRoleQueryProvider;

        private readonly IGetDataCutRuleFieldsQuery _rulesQuery;
        private readonly IGetDataCutResultTypeQuery _resultTypeQuery;
        private readonly IGetPropertyDisplayStrategiesDictionaryQuery _propertyDisplayStrategiesDictionaryQuery;
        private readonly IGetCountyQuery _countyQuery;

        public DictionaryService(IMapper mapper,
            IGetDataCutRuleFieldsQuery dataCutQuery,
            IGetDataCutResultTypeQuery resultTypeQuery,
            IQueryProvider<DAL.Queries.Entities.Event> eventQueryProvider,
            IQueryProvider<DAL.Queries.Entities.GeneralLandUseCode> generalLandUseCodeQueryProvider,
            IQueryProvider<DAL.Queries.Entities.InternalLandUseCode> internalLandUseCodeQueryProvider,
            IQueryProvider<DAL.Queries.Entities.PropertyProfileRuleField> propertyProfileRuleFieldQueryProvider,
            IQueryProvider<DAL.Queries.Entities.DepartmentRole> departmentRolesQueryProvider,
            IQueryProvider<DAL.Queries.Entities.UserRole> userRoleQueryProvider,
            IGetPropertyDisplayStrategiesDictionaryQuery propertyDisplayStrategiesDictionaryQuery,
            IGetCountyQuery countyQuery)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._rulesQuery = dataCutQuery ?? throw new ArgumentNullException(nameof(dataCutQuery));
            this._resultTypeQuery = resultTypeQuery ?? throw new ArgumentNullException(nameof(resultTypeQuery));
            this._eventQueryProvider = eventQueryProvider ?? throw new ArgumentNullException(nameof(eventQueryProvider));
            this._generalLandUseCodeQueryProvider = generalLandUseCodeQueryProvider ?? throw new ArgumentNullException(nameof(generalLandUseCodeQueryProvider));
            this._internalLandUseCodeQueryProvider = internalLandUseCodeQueryProvider ?? throw new ArgumentNullException(nameof(internalLandUseCodeQueryProvider));
            this._propertyProfileRuleFieldQueryProvider = propertyProfileRuleFieldQueryProvider ?? throw new ArgumentNullException(nameof(propertyProfileRuleFieldQueryProvider));
            this._departmentRolesQueryProvider = departmentRolesQueryProvider ?? throw new ArgumentNullException(nameof(departmentRolesQueryProvider));
            this._userRoleQueryProvider = userRoleQueryProvider ?? throw new ArgumentNullException(nameof(userRoleQueryProvider));
            this._propertyDisplayStrategiesDictionaryQuery = propertyDisplayStrategiesDictionaryQuery ?? throw new ArgumentNullException(nameof(propertyDisplayStrategiesDictionaryQuery));
            this._countyQuery = countyQuery ?? throw new ArgumentNullException(nameof(countyQuery));
        }

        public async Task<SearchResultModel<FastEntityModel<int>>> GetEnumDictionaryAsync<TEnum>(CancellationToken cancellationToken = default(CancellationToken))
            where TEnum : Enum
        {
            var items = this._mapper.Map<IEnumerable<FastEntityModel<int>>>(Enum.GetValues(typeof(TEnum)).Cast<Enum>());
            return await Task.FromResult(new SearchResultModel<FastEntityModel<int>>
            {
                TotalCount = items.Count(),
                List = items,
            }).ConfigureAwait(false);
        }

        public async Task<SearchResultModel<FastEntityModel<int>>> GetGeneralLandUseCodesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._generalLandUseCodeQueryProvider.Query;
            var list = await _mapper.ProjectTo<FastEntityModel<int>>(query).ToListAsync(cancellationToken).ConfigureAwait(false);

            return new SearchResultModel<FastEntityModel<int>>
            {
                TotalCount = list?.Count ?? 0,
                List = list,
            };
        }

        public async Task<SearchResultModel<FastEntityModel<int>>> GetInternalLandUseCodesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._internalLandUseCodeQueryProvider.Query;
            var list = await _mapper.ProjectTo<FastEntityModel<int>>(query).ToListAsync(cancellationToken).ConfigureAwait(false);

            return new SearchResultModel<FastEntityModel<int>>
            {
                TotalCount = list?.Count ?? 0,
                List = list,
            };
        }

        public async Task<SearchResultModel<FastEntityModel<int>>> GetDisplayStrategiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._propertyDisplayStrategiesDictionaryQuery;
            var items = await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            var count = items?.Count() ?? 0;

            return new SearchResultModel<FastEntityModel<int>>
            {
                TotalCount = count,
                List = this._mapper.Map<IEnumerable<FastEntityModel<int>>>(items),
            };
        }

        public async Task<SearchResultModel<DataCutRuleFieldModel>> GetDataCutItemListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._rulesQuery;
            var items = await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            var count = items?.Count() ?? 0;

            return new SearchResultModel<DataCutRuleFieldModel>
            {
                TotalCount = count,
                List = this._mapper.Map<IEnumerable<DataCutRuleFieldModel>>(items),
            };
        }

        public async Task<SearchResultModel<FastEntityModel<int>>> GetResultTypesListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._resultTypeQuery;
            var items = await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false);
            var count = items?.Count() ?? 0;

            return new SearchResultModel<FastEntityModel<int>>
            {
                TotalCount = count,
                List = this._mapper.Map<IEnumerable<FastEntityModel<int>>>(items),
            };
        }

        public async Task<SearchResultModel<FastEntityModel<Guid>>> GetEventListAsync(SearchArgsModel<EventDictionaryFilterArgs, EventDictionarySortField> args, CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<DAL.Queries.Entities.Event> query = this._eventQueryProvider.Query.Where(x => x.DeletedOn == null);

            if (string.IsNullOrWhiteSpace(args?.FullSearch) == false)
            {
                var val = args.FullSearch.Trim().ToLower(CultureInfo.InvariantCulture);
                query = query.Where(x => x.EventNumber.ToLower().Contains(val));
            }

            if (string.IsNullOrWhiteSpace(args?.Filter?.County) == false)
            {
                var val = args.Filter.County.Trim().ToLower(CultureInfo.InvariantCulture);
                query = query.Where(x => x.County.Name.ToLower().StartsWith(val));
            }

            if (args?.Filter?.StateIds?.Any() == true)
            {
                var ids = args.Filter.StateIds;
                query = query.Where(x => ids.Contains(x.StateId));
            }

            if (args?.Filter?.IsLocked != null)
            {
                var val = args.Filter.IsLocked;
                query = query.Where(x => x.IsLocked == val);
            }

            var count = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            var field = this.ResolveEventListSortExpression(args?.SortField ?? EventDictionarySortField.Number);
            query = (args?.SortOrder ?? SortOrder.Asc) == SortOrder.Asc ? query.OrderBy(field) : query.OrderByDescending(field);

            query = query.Skip(args?.Offset ?? 0).Take(args?.Limit ?? 50);

            var list = await this._mapper.ProjectTo<FastEntityModel<Guid>>(query).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new SearchResultModel<FastEntityModel<Guid>>
            {
                TotalCount = count,
                List = list,
            };
        }

        public async Task<SearchResultModel<FastEntityModel<int>>> GetCountiesAsync(SearchArgsModel<CountySearchArgs, CountySortField> args, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = this._countyQuery;

            if (string.IsNullOrWhiteSpace(args?.FullSearch) == false)
            {
                var val = args.FullSearch.Trim();
                query.Search(val);
            }

            var statId = args?.Filter?.StateId;
            if (statId.HasValue == true)
            {
                query.FilterByState(statId.Value);
            }

            query.Skip(args?.Offset ?? 0).Take(args?.Limit ?? 50);

            var items = await query.ExeсuteAsync(cancellationToken).ConfigureAwait(false);

            var count = query.TotalCount ?? 0;

            return new SearchResultModel<FastEntityModel<int>>
            {
                TotalCount = count,
                List = this._mapper.Map<IEnumerable<FastEntityModel<int>>>(items),
            };
        }

        public async Task<SearchResultModel<PropertyProfileRuleFieldModel>> GetPropertyProfileRuleFieldsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<DAL.Queries.Entities.PropertyProfileRuleField> query = this._propertyProfileRuleFieldQueryProvider.Query.Include(x => x.PropertyProfileFieldType).ThenInclude(x => x.PropertyProfileLogicTypes).Where(x => x.DeletedOn == null);

            var list = await query
                .Select(x => new PropertyProfileRuleFieldModel()
                {
                    Id = x.Id,
                    Name = x.Description,
                    PropertyProfileFieldType = new FastEntityModel<int>()
                    {
                        Id = x.PropertyProfileFieldTypeId,
                        Name = x.PropertyProfileFieldType.Description,
                    },
                    PropertyProfileLogicTypes = x.PropertyProfileFieldType.PropertyProfileLogicTypes.Select(a => new FastEntityModel<int>()
                    {
                        Id = a.Id,
                        Name = a.Description,
                    }).ToList(),
                }).ToListAsync(cancellationToken).ConfigureAwait(false);

            return new SearchResultModel<PropertyProfileRuleFieldModel>
            {
                TotalCount = list.Count,
                List = list,
            };
        }

        public async Task<SearchResultModel<DepartmentUsersModel>> GetDepartmentUsersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            List<DAL.Queries.Entities.DepartmentRole> departmentRoles = await this._departmentRolesQueryProvider.Query.Include(x => x.Department)
                .Where(x => x.DeletedOn == null).ToListAsync(cancellationToken).ConfigureAwait(false);
            List<DAL.Queries.Entities.UserRole> userRoles = await this._userRoleQueryProvider.Query.Include(x => x.User)
                .Where(x => x.DeletedOn == null && departmentRoles.Any(d => d.RoleId == x.RoleId) && x.User.IsActive && x.User.DeletedOn == null)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var list = departmentRoles
                        .GroupBy(x => x.DepartmentId)
                        .Select(x => new DepartmentUsersModel()
                        {
                            Id = x.Key,
                            Name = x.First().Department.Description,
                            Users = userRoles.Where(ur => x.Any(dr => dr.RoleId == ur.RoleId) && ur.User.IsActive && ur.User.DeletedOn == null)
                            .Select(ur => new FastEntityModel<Guid>()
                            {
                                Id = ur.UserId,
                                Name = $"{ur.User.FirstName} {ur.User.LastName}",
                            }).Distinct(new FastEntityComparer<Guid>()).ToList(),
                        });

            return new SearchResultModel<DepartmentUsersModel>
            {
                TotalCount = list.Count(),
                List = list,
            };
        }

        private Expression<Func<DAL.Queries.Entities.Event, object>> ResolveEventListSortExpression(EventDictionarySortField field)
        {
            switch (field)
            {
                case EventDictionarySortField.Number:
                    return e => e.EventNumber;
                default:
                    throw new ArgumentOutOfRangeException(nameof(field));
            }
        }

        private class FastEntityComparer<T> : IEqualityComparer<FastEntityModel<T>>
        {
            public bool Equals(FastEntityModel<T> x, FastEntityModel<T> y)
            {
                if (x.Id.Equals(y.Id))
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode(FastEntityModel<T> obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
