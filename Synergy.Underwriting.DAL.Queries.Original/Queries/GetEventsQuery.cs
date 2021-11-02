using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Queries
{
    public class GetEventsQuery : BaseQuery<Event>, IGetEventsQuery
    {
        private readonly IMapper _mapper;
        private readonly DbSet<Event> _entity;

        public GetEventsQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.Event;
        }

        public int? TotalCount { get; private set; }

        public IGetEventsQuery FindById(Guid id)
        {
            andAlsoPredicates.Add(u => u.Id == id);
            return this;
        }

        public IGetEventsQuery FindByUserId(Guid userId)
        {
            andAlsoPredicates.Add(u => u.UserId == userId);
            return this;
        }

        public IGetEventsQuery Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public IGetEventsQuery Take(int take)
        {
            _take = take;
            return this;
        }

        public IGetEventsQuery OrderBy(EventSortField sortField)
        {
            _isSortAsc = true;
            SetSortSelector(sortField);

            return this;
        }

        public IGetEventsQuery OrderByDescending(EventSortField sortField)
        {
            _isSortAsc = false;
            SetSortSelector(sortField);

            return this;
        }

        public IGetEventsQuery FilterByEventType(int eventTypeId)
        {
            andAlsoPredicates.Add(u => u.EventTypeId == eventTypeId);
            return this;
        }

        public IGetEventsQuery FilterByState(int stateId)
        {
            andAlsoPredicates.Add(u => u.StateId == stateId);
            return this;
        }

        public IGetEventsQuery FilterByStatus(bool isLocked)
        {
            andAlsoPredicates.Add(u => u.IsLocked == isLocked);
            return this;
        }

        public IGetEventsQuery IncludeAttachments()
        {
            includes.Add(e => e.EventAttachments);
            return this;
        }

        public IGetEventsQuery Search(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return this;
            }

            if (DateTime.TryParse(search, out DateTime date) && date != DateTime.MinValue)
            {
                andAlsoPredicates.Add(x => (x.SaleDate.Date == date.Date) || (x.FundingDate.Value.Date == date.Date));
                return this;
            }

            search = search.Trim().ToLower(CultureInfo.InvariantCulture);

            andAlsoPredicates.Add(x =>
                 x.State.Abbreviation.ToLower().StartsWith(search)
                || x.County.Name.ToLower().Contains(search)
                || x.EventType.Description.ToLower().Contains(search)
                || x.EventNumber.ToLower().Contains(search));

            return this;
        }

        public IEnumerable<EventModel> Exeсute()
        {
            IEnumerable<Event> data = BuildQuery();

            if (_skip != null || _take != null)
            {
                TotalCount = _entity.Where(GetPredicate()).Count();
            }

            foreach (var item in data)
            {
                item.EventAttachments = item.EventAttachments?.Where(x => x.DeletedOn == null);
            }

            return _mapper.Map<IEnumerable<EventModel>>(data);
        }

        public async Task<IEnumerable<EventModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<Event> data = BuildQuery();

            if (_skip != null || _take != null)
            {
                TotalCount = await _entity.Where(GetPredicate()).CountAsync().ConfigureAwait(false);
            }

            var result = await data.ToListAsync().ConfigureAwait(false);

            foreach (var eventItem in result)
            {
                eventItem.EventAttachments = eventItem.EventAttachments?.Where(a => a.DeletedOn == null);
            }

            return _mapper.Map<IEnumerable<EventModel>>(result);
        }

        private IQueryable<Event> BuildQuery()
        {
            includes.Add(e => e.User);
            includes.Add(e => e.State);
            includes.Add(e => e.County);
            includes.Add(e => e.EventType);
            includes.Add(e => e.EventDecisionLevels);
            includes.Add(e => e.EventUsers);

            IQueryable<Event> query = _entity
                .IncludeMultiple(includes.ToArray())
                .Where(GetPredicate())
                .OrderBy(_sortSelector, _isSortAsc)
                .ApplyPaging(_skip, _take);

            return query;
        }

        private void SetSortSelector(EventSortField sortField)
        {
            switch (sortField)
            {
                case EventSortField.CountyName:
                    _sortSelector = e => e.County.Name;
                    break;
                case EventSortField.CurrentTask:
                    _sortSelector = e => e.CurrentTask;
                    break;
                case EventSortField.DueDate:
                    _sortSelector = e => e.DueDate;
                    break;
                case EventSortField.EventNumber:
                    _sortSelector = e => e.EventNumber;
                    break;
                case EventSortField.EventType:
                    _sortSelector = e => e.EventType;
                    break;
                case EventSortField.Progress:
                    _sortSelector = e => e.Progress;
                    break;
                case EventSortField.SaleDate:
                    _sortSelector = e => e.SaleDate;
                    break;
                case EventSortField.State:
                    _sortSelector = e => e.State;
                    break;
                case EventSortField.AssignedTo:
                    _sortSelector = e => e.User.LastName;
                    break;
                case EventSortField.FundingDate:
                    _sortSelector = e => e.FundingDate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortField), "No sorting exist for such field");
            }
        }
    }
}
