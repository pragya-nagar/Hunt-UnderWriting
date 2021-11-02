using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Queries
{
    public class GetEventDelinquenciesQuery : BaseQuery<Delinquency>, IGetEventDelinquenciesQuery
    {
        private readonly IMapper _mapper;
        private readonly DbSet<Delinquency> _entity;
        private readonly ISynergyContext _context;

        public GetEventDelinquenciesQuery(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._entity = context.Delinquency;
            this._context = context;
        }

        public int? TotalCount { get; private set; }

        private Guid? CurrentDelinquencyId { get; set; }

        private bool MoveForward { get; set; }

        public IGetEventDelinquenciesQuery IncludeValuation()
        {
            this.includes.Add(d => d.Property.PropertyValuations);
            return this;
        }

        public IGetEventDelinquenciesQuery IncludeLead()
        {
            this.includes.Add(d => d.Property.Lead);
            return this;
        }

        public IGetEventDelinquenciesQuery IncludeSupplementalEventData()
        {
            this.includes.Add(d => d.PropertySupplementalEventData);
            return this;
        }

        public IGetEventDelinquenciesQuery FilterByDecisionType(ReviewStatusFilters status, Guid levelId, Guid userId)
        {
            switch (status)
            {
                case ReviewStatusFilters.NotReviewed:
                    this.BuildExpression(null, levelId, userId);
                    break;
                case ReviewStatusFilters.Approve:
                    this.BuildExpression((int)DataAccess.Enum.DecisionType.Approve, levelId, userId);
                    break;
                case ReviewStatusFilters.Reject:
                    this.BuildExpression((int)DataAccess.Enum.DecisionType.Reject, levelId, userId);
                    break;
                case ReviewStatusFilters.Research:
                    this.BuildExpression((int)DataAccess.Enum.DecisionType.Research, levelId, userId);
                    break;
                case ReviewStatusFilters.AutoApprove:
                    this.andAlsoPredicates.Add(x => x.EventDataCutDecisions.Any(y => y.EventDataCutStrategy.IsActive && y.DecisionTypeId == (int)DataAccess.Enum.DecisionType.AutoApprove));
                    break;
                case ReviewStatusFilters.AutoReject:
                    this.andAlsoPredicates.Add(x => x.EventDataCutDecisions.Any(y => y.EventDataCutStrategy.IsActive && y.DecisionTypeId == (int)DataAccess.Enum.DecisionType.AutoReject));
                    break;
            }

            return this;
        }

        public IGetEventDelinquenciesQuery FilterByPriorDecision(ReviewDecisionSearchField decisionType, Guid userId)
        {
            switch (decisionType)
            {
                case ReviewDecisionSearchField.All:
                    break;
                case ReviewDecisionSearchField.WithPriorDecision:
                    this.andAlsoPredicates.Add(del => del.Decisions.Any(dfirst => (dfirst.EventDecisionLevel.Order == 0 && dfirst.UserId == userId)
                                  || del.Decisions.Any(d => d.DecisionTypeId != null
                                  && del.Decisions.Any(di => di.UserId == userId && d.EventDecisionLevel.Order == (di.EventDecisionLevel.Order - 1)))));
                    break;
                case ReviewDecisionSearchField.WithoutPriorDecision:
                    this.andAlsoPredicates.Add(del => del.Decisions.Any(dfirst => del.Decisions.Any(d => d.DecisionTypeId == null
                                  && del.Decisions.Any(di => di.UserId == userId && d.EventDecisionLevel.Order == (di.EventDecisionLevel.Order - 1)))));
                    break;
            }

            return this;
        }

        public IGetEventDelinquenciesQuery FindById(Guid id)
        {
            this.andAlsoPredicates.Add(e => e.Id == id);
            return this;
        }

        public IGetEventDelinquenciesQuery FilterByEventIds(List<Guid> eventIds)
        {
            this.andAlsoPredicates.Add(e => e.EventId != null && eventIds.Contains((Guid)e.EventId));
            return this;
        }

        public IGetEventDelinquenciesQuery FilterByInactiveDataCut()
        {
            this.andAlsoPredicates.Add(x => !x.EventDataCutDecisions.Any(y => y.EventDataCutStrategy.IsActive));
            return this;
        }

        public IGetEventDelinquenciesQuery Take(int take)
        {
            this._take = take;
            return this;
        }

        public IGetEventDelinquenciesQuery Skip(int skip)
        {
            this._skip = skip;
            return this;
        }

        public IGetEventDelinquenciesQuery FindByUserId(Guid userId)
        {
            this.andAlsoPredicates.Add(d => d.Decisions.Any(dd => dd.UserId == userId));
            return this;
        }

        public IGetEventDelinquenciesQuery FilterByPropertyFields(PropertyFieldsFilterModel fields)
        {
            if (fields == null)
            {
                return this;
            }

            if (fields.MinAmountDue.HasValue)
            {
                andAlsoPredicates.Add(x => x.Amount >= fields.MinAmountDue.Value);
            }

            if (fields.MaxAmountDue.HasValue)
            {
                andAlsoPredicates.Add(x => x.Amount <= fields.MaxAmountDue.Value);
            }

            if (fields.MinAssessedValue.HasValue)
            {
                this.andAlsoPredicates.Add(x => x.Property.PropertyValuations.Any(y => y.AppraisedValue.HasValue && y.AppraisedValue >= fields.MinAssessedValue.Value && y.IsActive));
            }

            if (fields.MaxAssessedValue.HasValue)
            {
                this.andAlsoPredicates.Add(x => x.Property.PropertyValuations.Any(y => y.AppraisedValue.HasValue && y.AppraisedValue <= fields.MaxAssessedValue.Value && y.IsActive));
            }

            if (fields.GeneralLandUseCodes.Any())
            {
                this.andAlsoPredicates.Add(d => d.Property.GeneralLandUseCodeId.HasValue && fields.GeneralLandUseCodes.Contains(d.Property.GeneralLandUseCodeId.Value));
            }

            if (fields.InternalLandUseCodes.Any())
            {
                this.andAlsoPredicates.Add(d => d.Property.InternalLandUseCodeId.HasValue && fields.InternalLandUseCodes.Contains(d.Property.InternalLandUseCodeId.Value));
            }

            if (!string.IsNullOrWhiteSpace(fields.LandUseCode))
            {
                this.andAlsoPredicates.Add(d => d.Property.LandUseCode.ToLower().Contains(fields.LandUseCode.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(fields.Owner))
            {
                this.andAlsoPredicates.Add(d => d.Property.Lead.AccountName.ToLower().Contains(fields.Owner.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(fields.ParcelID))
            {
                this.andAlsoPredicates.Add(d => d.Property.ParcelId.ToLower().Contains(fields.ParcelID.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(fields.PropertyAddress))
            {
                this.andAlsoPredicates.Add(d => d.Property.Address.ToLower().Contains(fields.PropertyAddress.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(fields.PropertyCity))
            {
                this.andAlsoPredicates.Add(d => d.Property.City.ToLower().Contains(fields.PropertyCity.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(fields.PropertyZipCode))
            {
                this.andAlsoPredicates.Add(d => d.Property.ZipCode.ToLower().StartsWith(fields.PropertyZipCode.ToLower()));
            }

            return this;
        }

        public IGetEventDelinquenciesQuery Search(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return this;
            }

            search = search.Trim().ToLower(CultureInfo.InvariantCulture);
            this.andAlsoPredicates.Add(d => d.Property.Lead.AccountName.ToLower().StartsWith(search) ||
                                        d.Property.ParcelId.ToLower().StartsWith(search) ||
                                        d.Property.Address.ToLower().Contains(search));

            return this;
        }

        public IGetEventDelinquenciesQuery FilterByDelinquencyId(Guid delinquencyId)
        {
            this.CurrentDelinquencyId = delinquencyId;

            return this;
        }

        public IGetEventDelinquenciesQuery SetOrder(bool moveForward)
        {
            this.MoveForward = moveForward;
            return this;
        }

        public IEnumerable<PropertyAssignmentModel> Exeсute()
        {
            List<Delinquency> delinquencies = GetDelinquency().ToList().OrderByDescending(d => d.Id).ToList();
            List<Guid> delinquencyIds = delinquencies.Select(d => d.Id).ToList();
            List<DelinquencyPropertyDisplayStrategy> displayStrategies = this.GetDisplayStrategies(delinquencyIds).ToList();
            List<DelinquencyPropertyScoring> scorings = this.GetScoring(delinquencyIds).ToList();
            List<Decision> decisions = this.GetDecisions(delinquencyIds).ToList();
            List<PropertyAttachment> attachments = this.GetAttachments(delinquencies.Select(x => x.PropertyId).ToList()).ToList();
            List<EventDataCutDecision> dataCutDecisions = this.GetDataCutDecision(delinquencyIds).ToList();

            var propertyIds = delinquencies.Select(x => x.PropertyId).Distinct();
            var propertyLatestEventIdList = this.GetLatestEventIdAsync(propertyIds).GetAwaiter().GetResult();

            List<PropertyAssignmentModel> results = new List<PropertyAssignmentModel>();
            foreach (Delinquency delinquency in delinquencies)
            {
                var property = this._mapper.Map<PropertyAssignmentModel>(delinquency.Property);
                property.Event = this._mapper.Map<EventModel>(delinquency.Event);
                property.IsLatestPropertyData = propertyLatestEventIdList[property.PropertyId].Item2 >= DateTime.UtcNow.Date || (propertyLatestEventIdList.ContainsKey(property.PropertyId) && propertyLatestEventIdList[property.PropertyId].Item1 == property.Event.Id);
                property.Id = delinquency.Id;
                property.DelinquencyYear = delinquency.DelinquencyTaxYear;
                property.TotalAmountDue = delinquency.Amount;
                property.RUAmount = delinquency.RUAmount;
                property.RULTV = delinquency.RULTVPercent;
                property.LTV = delinquency.LTVPercent;
                property.PropertySupplementalEventData = this._mapper.Map<PropertySupplementalEventDataModel>(delinquency.PropertySupplementalEventData);
                property.Decisions = this._mapper.Map<List<PropertyDecisionModel>>(decisions.Where(d => d.DelinquencyId == delinquency.Id).ToList());
                property.PropertyDisplayStrategiesIds = displayStrategies.Where(x => x.DelinquencyId == delinquency.Id).Select(x => x.PropertyDisplayStrategyId).ToList();
                property.PropertyScoring = scorings.Where(x => x.DelinquencyId == delinquency.Id).Select(x => x.PropertyScoring).SingleOrDefault();
                property.PropertyAttachments = this._mapper.Map<List<PropertyAttachmentModel>>(attachments.Where(x => x.PropertyId == delinquency.PropertyId).ToList());
                property.DataCutDecision = this._mapper.Map<FastEntityModel<int>>(dataCutDecisions.SingleOrDefault(x => x.DelinquencyId == delinquency.Id));
                results.Add(property);
            }

            return results;
        }

        public async Task<IEnumerable<PropertyAssignmentModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Delinquency> delinquencies = (await GetDelinquency().ToListAsync(cancellationToken).ConfigureAwait(false)).OrderByDescending(d => d.Id).ToList();
            List<Guid> delinquencyIds = delinquencies.Select(d => d.Id).ToList();
            List<DelinquencyPropertyDisplayStrategy> displayStrategies = await GetDisplayStrategies(delinquencyIds).ToListAsync(cancellationToken).ConfigureAwait(false);
            List<DelinquencyPropertyScoring> scorings = await GetScoring(delinquencyIds).ToListAsync(cancellationToken).ConfigureAwait(false);
            List<Decision> decisions = await GetDecisions(delinquencyIds).ToListAsync(cancellationToken).ConfigureAwait(false);
            List<PropertyAttachment> attachments = await GetAttachments(delinquencies.Select(x => x.PropertyId).ToList()).ToListAsync(cancellationToken).ConfigureAwait(false);
            List<EventDataCutDecision> dataCutDecisions = await GetDataCutDecision(delinquencyIds).ToListAsync(cancellationToken).ConfigureAwait(false);

            var propertyIds = delinquencies.Select(x => x.PropertyId).Distinct();
            var propertyLatestEventIdList = await this.GetLatestEventIdAsync(propertyIds, cancellationToken).ConfigureAwait(false);

            List<PropertyAssignmentModel> results = new List<PropertyAssignmentModel>();
            foreach (Delinquency delinquency in delinquencies)
            {
                var property = this._mapper.Map<PropertyAssignmentModel>(delinquency.Property);
                property.Event = this._mapper.Map<EventModel>(delinquency.Event);
                property.IsLatestPropertyData = propertyLatestEventIdList[property.PropertyId].Item2 >= DateTime.UtcNow.Date || (propertyLatestEventIdList.ContainsKey(property.PropertyId) && propertyLatestEventIdList[property.PropertyId].Item1 == property.Event.Id);
                property.Id = delinquency.Id;
                property.DelinquencyYear = delinquency.DelinquencyTaxYear;
                property.TotalAmountDue = delinquency.Amount;
                property.RUAmount = delinquency.RUAmount;
                property.RULTV = delinquency.RULTVPercent;
                property.LTV = delinquency.LTVPercent;
                property.PropertySupplementalEventData = this._mapper.Map<PropertySupplementalEventDataModel>(delinquency.PropertySupplementalEventData);
                property.Decisions = this._mapper.Map<List<PropertyDecisionModel>>(decisions.Where(d => d.DelinquencyId == delinquency.Id).ToList());
                property.PropertyDisplayStrategiesIds = displayStrategies.Where(x => x.DelinquencyId == delinquency.Id).Select(x => x.PropertyDisplayStrategyId).ToList();
                property.PropertyScoring = scorings.Where(x => x.DelinquencyId == delinquency.Id).Select(x => x.PropertyScoring).SingleOrDefault();
                property.PropertyAttachments = this._mapper.Map<List<PropertyAttachmentModel>>(attachments.Where(x => x.PropertyId == delinquency.PropertyId).ToList());
                property.DataCutDecision = this._mapper.Map<FastEntityModel<int>>(dataCutDecisions.SingleOrDefault(x => x.DelinquencyId == delinquency.Id));
                results.Add(property);
            }

            return results;
        }

        private IGetEventDelinquenciesQuery ApplyFilterByDelinquencyId()
        {
            if (!this.CurrentDelinquencyId.HasValue)
            {
                return this;
            }

            Guid delinquencyId = this.CurrentDelinquencyId.Value;
            if (this.MoveForward)
            {
                andAlsoPredicates.Add(x => string.Compare(x.Id.ToString(), delinquencyId.ToString()) < 0);
            }
            else
            {
                andAlsoPredicates.Add(x => string.Compare(x.Id.ToString(), delinquencyId.ToString()) > 0);
            }

            return this;
        }

        private async Task<IDictionary<Guid, (Guid, DateTime)>> GetLatestEventIdAsync(IEnumerable<Guid> propertyIds, CancellationToken cancellationToken = default)
        {
            var plainList = await (from d in this._context.Delinquency
                                   join e in this._context.Event on d.EventId equals e.Id
                                   where d.DeletedOn == null && e.DeletedOn == null && propertyIds.Contains(d.PropertyId)
                                   select new { d.PropertyId, d.EventId, e.SaleDate }).ToListAsync(cancellationToken).ConfigureAwait(false);

            return (from item in plainList
                    group new { item.EventId, item.SaleDate } by item.PropertyId into g
                    select new
                    {
                        Id = g.Key,
                        EventInfo = g.OrderByDescending(x => x.SaleDate).Select(x => (x.EventId.Value, x.SaleDate)).First(),
                    }).ToDictionary(x => x.Id, x => x.EventInfo);
        }

        private IQueryable<Delinquency> GetDelinquency()
        {
            this.includes.Add(d => d.Event);
            this.includes.Add(d => d.Property.InternalLandUseCode);
            this.includes.Add(d => d.Property.State);
            this.includes.Add(d => d.Property.County);
            this.includes.Add(d => d.Property.Lead.MailingState);

            if (this._skip != null && this._take != null)
            {
                this.TotalCount = this._entity.Where(this.GetPredicate()).Count();
            }

            this.ApplyFilterByDelinquencyId();
            var query = _entity
                .IncludeMultiple(includes.ToArray())
                .Where(GetPredicate());

            if (this.MoveForward)
            {
                query = query.OrderByDescending(d => d.Id);
            }
            else
            {
                query = query.OrderBy(d => d.Id);
            }

            query = query.ApplyPaging(_skip, _take);

            return query;
        }

        private IQueryable<DelinquencyPropertyDisplayStrategy> GetDisplayStrategies(List<Guid> delinquencyIds)
        {
            return this._context.DelinquencyPropertyDisplayStrategy.Where(x => delinquencyIds.Any(d => d == x.DelinquencyId));
        }

        private IQueryable<DelinquencyPropertyScoring> GetScoring(List<Guid> delinquencyIds)
        {
            return this._context.DelinquencyPropertyScoring.Where(x => delinquencyIds.Any(d => d == x.DelinquencyId));
        }

        private IQueryable<Decision> GetDecisions(List<Guid> delinaquencyIds)
        {
            IQueryable<Decision> query = this._context.Decision
                .Include(d => d.EventDecisionLevel)
                .Where(edl => delinaquencyIds.Contains(edl.DelinquencyId));

            return query;
        }

        private IQueryable<PropertyAttachment> GetAttachments(List<Guid> propertyIds)
        {
            IQueryable<PropertyAttachment> query = this._context.PropertyAttachment.Where(pa => propertyIds.Contains(pa.PropertyId) && pa.DeletedOn == null);
            return query;
        }

        private IQueryable<EventDataCutDecision> GetDataCutDecision(List<Guid> delinaquencyIds)
        {
            IQueryable<EventDataCutDecision> query = this._context.EventDataCutDecision
                .Include(d => d.DecisionType)
                .Where(edd => delinaquencyIds.Contains(edd.DelinquencyId) && edd.EventDataCutStrategy.IsActive);
            return query;
        }

        private void BuildExpression(int? decisionTypeId, Guid levelId, Guid userId)
        {
            this.andAlsoPredicates.Add(x => x.Decisions.Any(d => d.DecisionTypeId.Value == decisionTypeId
                                    && (levelId == Guid.Empty ? true : d.EventDecisionLevelId == levelId)
                                    && (userId == Guid.Empty ? true : d.UserId == userId))
                                    && x.EventDataCutDecisions.All(y => y.EventDataCutStrategy.IsActive == false));
        }
    }
}
