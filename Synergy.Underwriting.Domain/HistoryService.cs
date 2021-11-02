using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.Common.Abstracts;
using Synergy.Common.DAL.Abstract;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.Exceptions;
using Synergy.Underwriting.Domain.Abstracts;
using Synergy.Underwriting.Domain.Models.History;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.Domain
{
    public class HistoryService : IHistoryService
    {
        private readonly Guid _systemUserId = Common.Constants.User.SystemUserId;

        private readonly Dictionary<string, string[]> entityFields = new Dictionary<string, string[]>()
        {
            { "Property",  new string[] { "CountyName", "LandUseCode", "GeneralLandUseCodeName", "InternalLandUseCodeName", "City", "Address", "ZipCode", "StateName", "ParcelId", "LeadName", "Homestead", "LandAcres", "BuildingSqFt", "YearBuilt" } },
            { "Delinquency",  new string[] { "Amount", "RUAmount", "DelinquencyTaxYear", "LTVPercent", "RULTVPercent" } },
            {
                "PropertySupplementalEventData", new string[]
                {
                    "LastSaleDate", "LastSaleAmount", "LastSaleDate", "LastSaleAmount", "MortgageLoanAmount1", "MortgageLoanAmount2",
                    "MortgageOriginationDate1", "MortgageOriginationDate2", "MortgageMaturityDate1", "MortgageMaturityDate2", "OpenLiens", "ClosedLiens", "RecentBuyerName",
                    "RecentBuyerRate", "InspectorComment", "InspectorPropertyRating", "InspectorAreaRating", "InspectorOccupied", "InspectorRoofCondition", "InspectorLawnMaintained",
                }
            },
            { "Lead", new string[] { "MailingAddress1", "MailingAddress2", "MailingAddress3", "MailingCity", "MailingState", "MailingZipCode" } },
            { "DelinquencyPropertyDisplayStrategy", new string[] { "DispositionStrategy" } },
            { "PropertyValuation", new string[] { "LandValue", "ImprovementValue", "AppraisedValue" } },
        };

        private readonly IMapper _mapper;
        private readonly IClockService _clockService;

        private readonly IQueryProvider<DAL.Queries.Entities.DelinquencyAudit> _delinquencyAuditQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.Delinquency> _delinquencyQueryProvider;

        private readonly IQueryProvider<DAL.Queries.Entities.PropertyAudit> _propertyAuditQueryProvider;

        private readonly IQueryProvider<DAL.Queries.Entities.SupplementalDataAudit> _supplementalDataAuditQueryProvider;

        private readonly IQueryProvider<DAL.Queries.Entities.LeadAudit> _leadAuditQueryProvider;

        private readonly IQueryProvider<DAL.Queries.Entities.PropertyDisplayStrategy> _displayStrategyQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.DelinquencyPropertyDisplayStrategyAudit> _displayStrategyAuditQueryProvider;

        private readonly IQueryProvider<DAL.Queries.Entities.PropertyValuation> _propertyValuationQueryProvider;
        private readonly IQueryProvider<DAL.Queries.Entities.PropertyValuationAudit> _propertyValuationAuditQueryProvider;

        private readonly IQueryProvider<DAL.Queries.Entities.User> _userQueryProvider;

        public HistoryService(
            IMapper mapper,
            IClockService clockService,
            IQueryProvider<DAL.Queries.Entities.DelinquencyAudit> delinquencyAuditQueryProvider,
            IQueryProvider<DAL.Queries.Entities.Delinquency> delinquencyQueryProvider,
            IQueryProvider<DAL.Queries.Entities.PropertyAudit> propertyAuditQueryProvider,
            IQueryProvider<DAL.Queries.Entities.SupplementalDataAudit> supplementalDataAuditQueryProvider,
            IQueryProvider<DAL.Queries.Entities.LeadAudit> leadAuditQueryProvider,
            IQueryProvider<DAL.Queries.Entities.PropertyDisplayStrategy> displayStrategyQueryProvider,
            IQueryProvider<DAL.Queries.Entities.DelinquencyPropertyDisplayStrategyAudit> displayStrategyAuditQueryProvider,
            IQueryProvider<DAL.Queries.Entities.PropertyValuationAudit> propertyValuationAuditQueryProvider,
            IQueryProvider<DAL.Queries.Entities.PropertyValuation> propertyValuationQueryProvider,
            IQueryProvider<DAL.Queries.Entities.User> userQueryProvider)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._clockService = clockService ?? throw new ArgumentNullException(nameof(clockService));

            this._delinquencyAuditQueryProvider = delinquencyAuditQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyAuditQueryProvider));
            this._delinquencyQueryProvider = delinquencyQueryProvider ?? throw new ArgumentNullException(nameof(delinquencyQueryProvider));
            this._propertyAuditQueryProvider = propertyAuditQueryProvider ?? throw new ArgumentNullException(nameof(propertyAuditQueryProvider));
            this._supplementalDataAuditQueryProvider = supplementalDataAuditQueryProvider ?? throw new ArgumentNullException(nameof(supplementalDataAuditQueryProvider));
            this._leadAuditQueryProvider = leadAuditQueryProvider ?? throw new ArgumentNullException(nameof(leadAuditQueryProvider));
            this._displayStrategyQueryProvider = displayStrategyQueryProvider ?? throw new ArgumentNullException(nameof(displayStrategyQueryProvider));
            this._displayStrategyAuditQueryProvider = displayStrategyAuditQueryProvider ?? throw new ArgumentNullException(nameof(displayStrategyAuditQueryProvider));
            this._propertyValuationAuditQueryProvider = propertyValuationAuditQueryProvider ?? throw new ArgumentNullException(nameof(propertyValuationAuditQueryProvider));
            this._propertyValuationQueryProvider = propertyValuationQueryProvider ?? throw new ArgumentNullException(nameof(propertyValuationQueryProvider));
            this._userQueryProvider = userQueryProvider ?? throw new ArgumentNullException(nameof(userQueryProvider));
        }

        public async Task<SearchResultModel<HistoryModel>> GetListAsync(Guid id, FilterModel filterModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            filterModel.DateFrom = filterModel.DateFrom.ToUniversalTime();
            filterModel.DateTo = filterModel.DateTo.ToUniversalTime();

            List<HistoryModel> histories = new List<HistoryModel>();

            var delinquency = await this._delinquencyQueryProvider.Query
                                            .Include(p => p.Property).ThenInclude(p => p.Lead).ThenInclude(l => l.State)
                                            .Include(p => p.Property).ThenInclude(p => p.State)
                                            .Include(p => p.Property).ThenInclude(p => p.County)
                                            .Include(p => p.Property).ThenInclude(p => p.GeneralLandUseCode)
                                            .Include(p => p.Property).ThenInclude(p => p.InternalLandUseCode)
                                            .Include(s => s.SupplementalData)
                                            .Include(d => d.DelinquencyPropertyDisplayStrategy).ThenInclude(x => x.PropertyDisplayStrategy)
                                            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                                            .ConfigureAwait(false);

            if (delinquency == null)
            {
                throw new NotFoundException();
            }

            var delinquencyAudits = await this._delinquencyAuditQueryProvider.Query
                                            .Where(x => x.InsertedBy != Guid.Empty && x.DeletedOn == null && x.Id == id && x.InsertedOn >= filterModel.DateFrom && x.InsertedOn <= filterModel.DateTo)
                                            .ToListAsync(cancellationToken)
                                            .ConfigureAwait(false);

            var propertyAudits = await this._propertyAuditQueryProvider.Query
                                            .Include(s => s.State)
                                            .Include(s => s.County)
                                            .Include(s => s.Lead)
                                            .Include(s => s.GeneralLandUseCode)
                                            .Include(s => s.InternalLandUseCode)
                                            .Where(x => x.InsertedBy != Guid.Empty && x.DeletedOn == null && x.Id == delinquency.PropertyId && x.InsertedOn >= filterModel.DateFrom && x.InsertedOn <= filterModel.DateTo)
                                            .ToListAsync(cancellationToken)
                                            .ConfigureAwait(false);

            var supplementalDataAudits = await this._supplementalDataAuditQueryProvider.Query
                                .Where(x => x.InsertedBy != Guid.Empty && x.DeletedOn == null && x.DelinquencyId == delinquency.Id && x.InsertedOn >= filterModel.DateFrom && x.InsertedOn <= filterModel.DateTo)
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false);

            var leadAuditModels = new List<LeadAuditModel>();

            var leads = propertyAudits.Select(x => new { Id = x.LeadId, Date = x.InsertedOn })
                .Union(new[] { new { Id = delinquency.Property.LeadId, Date = delinquency.Property.ModifiedOn } })
                .GroupBy(x => x.Id)
                .Select(x => new { Id = x.Key, Date = x.Max(y => y.Date) })
                .OrderBy(x => x.Date);

            var startDate = filterModel.DateFrom;
            foreach (var lead in leads)
            {
                var historyPart = await this._leadAuditQueryProvider.Query
                    .Include(x => x.State).Where(x => x.InsertedBy != Guid.Empty && x.DeletedOn == null && x.Id == lead.Id && x.InsertedOn >= startDate && x.InsertedOn <= lead.Date)
                    .Select(x => new LeadAuditModel
                    {
                        InsertedOn = x.InsertedOn,
                        InsertedBy = x.InsertedBy,
                        OperationId = x.OperationId,

                        Id = x.Id,
                        CreatedOn = x.CreatedOn,
                        CreatedById = x.CreatedById,
                        ModifiedOn = x.ModifiedOn,
                        ModifiedById = x.ModifiedById,
                        DeletedOn = x.DeletedOn,

                        MailingState = x.State.Name,
                        MailingAddress1 = x.MailingAddress1,
                        MailingAddress2 = x.MailingAddress2,
                        MailingAddress3 = x.MailingAddress3,
                        MailingCity = x.MailingCity,
                        MailingZipCode = x.MailingZipCode,
                    })
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
                leadAuditModels.AddRange(historyPart);

                startDate = lead.Date;
            }

            var currentLead = new LeadAuditModel
            {
                Id = delinquency.Property.Lead.Id,
                CreatedOn = delinquency.Property.Lead.CreatedOn,
                CreatedById = delinquency.Property.Lead.CreatedById,
                ModifiedOn = delinquency.Property.Lead.ModifiedOn,
                ModifiedById = delinquency.Property.Lead.ModifiedById,
                DeletedOn = delinquency.Property.Lead.DeletedOn,

                MailingState = delinquency.Property.Lead.State?.Name ?? string.Empty,
                MailingAddress1 = delinquency.Property.Lead.MailingAddress1,
                MailingAddress2 = delinquency.Property.Lead.MailingAddress2,
                MailingAddress3 = delinquency.Property.Lead.MailingAddress3,
                MailingCity = delinquency.Property.Lead.MailingCity,
                MailingZipCode = delinquency.Property.Lead.MailingZipCode,
            };

            var displayStrategyAuditList = await BuildDisplayStrategyListAsync(id, delinquency.DelinquencyPropertyDisplayStrategy, filterModel, cancellationToken).ConfigureAwait(false);

            var propertyValuation = await this._propertyValuationQueryProvider.Query
                                .OrderByDescending(t => t.AppraisedYear)
                                .FirstOrDefaultAsync(x => x.PropertyId == delinquency.PropertyId)
                                .ConfigureAwait(false);

            histories.AddRange(this.AddHistories(delinquencyAudits, this._mapper.Map<DAL.Queries.Entities.Delinquency, DAL.Queries.Entities.DelinquencyAudit>(delinquency), filterModel, this.entityFields["Delinquency"]));
            histories.AddRange(this.AddHistories(propertyAudits, this._mapper.Map<DAL.Queries.Entities.Property, DAL.Queries.Entities.PropertyAudit>(delinquency.Property), filterModel, this.entityFields["Property"]));
            histories.AddRange(this.AddHistories(supplementalDataAudits, this._mapper.Map<DAL.Queries.Entities.SupplementalData, DAL.Queries.Entities.SupplementalDataAudit>(delinquency.SupplementalData), filterModel, this.entityFields["PropertySupplementalEventData"]));
            histories.AddRange(this.AddHistories(leadAuditModels, currentLead, filterModel, this.entityFields["Lead"]));
            histories.AddRange(this.BuildCollectionHistory(displayStrategyAuditList, filterModel, entityFields["DelinquencyPropertyDisplayStrategy"]));

            if (propertyValuation != null)
            {
                var propertyValuationAudits = await this._propertyValuationAuditQueryProvider.Query
                               .Where(x => x.InsertedBy != Guid.Empty && x.DeletedOn == null && x.Id == propertyValuation.Id && x.InsertedOn >= filterModel.DateFrom && x.InsertedOn <= filterModel.DateTo)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false);
                histories.AddRange(this.AddHistories(propertyValuationAudits, this._mapper.Map<DAL.Queries.Entities.PropertyValuation, DAL.Queries.Entities.PropertyValuationAudit>(propertyValuation), filterModel, this.entityFields["PropertyValuation"]));
            }

            var userIds = histories.Select(t => t.UserId).Distinct();
            var users = await this._userQueryProvider.Query
                                            .Where(x => userIds.Contains(x.Id))
                                            .ToDictionaryAsync(x => x.Id, x => x)
                                            .ConfigureAwait(false);

            foreach (var item in histories)
            {
                if (item.UserId == this._systemUserId)
                {
                    item.UserName = "System User";
                    continue;
                }

                if (users.ContainsKey(item.UserId) == false)
                {
                    item.UserName = "Unknown user: 'delete' operation";
                    continue;
                }

                var user = users[item.UserId];
                item.UserName = $"{user.FirstName} {user.LastName}";
            }

            return new SearchResultModel<HistoryModel>()
            {
                TotalCount = histories.Count(),
                List = histories,
            };
        }

        private async Task<IEnumerable<DisplayStrategyAuditModel>> BuildDisplayStrategyListAsync(Guid delinquencyId, IEnumerable<DAL.Queries.Entities.DelinquencyPropertyDisplayStrategy> currentCollection, FilterModel filterModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            var adjustedDateTo = filterModel.DateTo;

            var mainList = await this._displayStrategyAuditQueryProvider.Query
                .Where(x => x.InsertedBy != Guid.Empty && x.DelinquencyId == delinquencyId && x.InsertedOn >= filterModel.DateFrom && x.InsertedOn <= filterModel.DateTo)
                .Include(x => x.PropertyDisplayStrategy)
                .Select(x => new DisplayStrategyAuditModel
                {
                    InsertedOn = x.InsertedOn,
                    InsertedBy = x.InsertedBy,
                    OperationId = x.OperationId,

                    CreatedOn = x.CreatedOn,
                    CreatedById = x.CreatedById,

                    DispositionStrategy = x.PropertyDisplayStrategy.Description,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (mainList.Count != 0)
            {
                var firstOperationId = mainList.Where(x => x.InsertedOn == mainList.Min(y => y.InsertedOn)).First().OperationId;
                var lastOperationId = mainList.Where(x => x.InsertedOn == mainList.Max(y => y.InsertedOn)).First().OperationId;

                var adjustmentList = await this._displayStrategyAuditQueryProvider.Query
                .Where(x => x.OperationId == firstOperationId || x.OperationId == lastOperationId)
                .Include(x => x.PropertyDisplayStrategy)
                .Select(x => new DisplayStrategyAuditModel
                {
                    InsertedOn = x.InsertedOn,
                    InsertedBy = x.InsertedBy,
                    OperationId = x.OperationId,

                    CreatedOn = x.CreatedOn,
                    CreatedById = x.CreatedById,

                    DispositionStrategy = x.PropertyDisplayStrategy.Description,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

                mainList.Union(adjustmentList);

                var maxInsertDate = mainList.Max(y => y.InsertedOn);
                adjustedDateTo = adjustedDateTo < maxInsertDate ? maxInsertDate : adjustedDateTo;
            }

            var nextAuditList = new List<DisplayStrategyAuditModel>();
            var nextOperationId = await (from displayStrategyAuditSub in this._displayStrategyAuditQueryProvider.Query
                                              where displayStrategyAuditSub.InsertedOn > adjustedDateTo
                                              orderby displayStrategyAuditSub.InsertedOn
                                              select displayStrategyAuditSub.OperationId)
                                              .FirstOrDefaultAsync().ConfigureAwait(false);

            if (nextOperationId != Guid.Empty)
            {
                nextAuditList = await (from displayStrategyAudit in this._displayStrategyAuditQueryProvider.Query
                                           join displayStrategy in this._displayStrategyQueryProvider.Query on displayStrategyAudit.PropertyDisplayStrategyId equals displayStrategy.Id
                                           where displayStrategyAudit.OperationId == nextOperationId
                                           select new DisplayStrategyAuditModel
                                           {
                                               InsertedOn = displayStrategyAudit.InsertedOn,
                                               InsertedBy = displayStrategyAudit.InsertedBy,
                                               OperationId = displayStrategyAudit.OperationId,

                                               CreatedOn = displayStrategyAudit.CreatedOn,
                                               CreatedById = displayStrategyAudit.CreatedById,

                                               DispositionStrategy = displayStrategy.Description,
                                           })
                                           .ToListAsync().ConfigureAwait(false);
            }

            if (nextAuditList.Count != 0)
            {
                mainList.AddRange(nextAuditList);
            }
            else if (currentCollection.Count() != 0)
            {
                mainList.AddRange(currentCollection.Select(x =>
                {
                    return new DisplayStrategyAuditModel
                    {
                        InsertedOn = mainList.Count != 0 ? mainList.Max(y => y.InsertedOn).AddMilliseconds(1) : x.CreatedOn,
                        InsertedBy = x.CreatedById,
                        OperationId = Guid.Empty,

                        CreatedOn = x.CreatedOn,
                        CreatedById = x.CreatedById,

                        DispositionStrategy = x.PropertyDisplayStrategy.Description,
                    };
                }).ToList());
            }
            else if (mainList.Count != 0)
            {
                var maxInsertDate = mainList.Max(y => y.InsertedOn);
                var insertedBy = mainList.Where(x => x.InsertedOn == maxInsertDate).First().InsertedBy;

                mainList.AddRange(new List<DisplayStrategyAuditModel>
                {
                    new DisplayStrategyAuditModel
                    {
                        InsertedOn = maxInsertDate.AddMilliseconds(1),
                        InsertedBy = insertedBy,
                        OperationId = Guid.Empty,

                        CreatedOn = maxInsertDate,
                        CreatedById = insertedBy,

                        DispositionStrategy = string.Empty,
                    },
                });
            }

            return mainList;
        }

        private IEnumerable<HistoryModel> BuildCollectionHistory<T>(IEnumerable<T> collectionAuditList, FilterModel fm, string[] entityFields)
            where T : DAL.Queries.Entities.History.IHistoryAuditModel<Guid>
        {
            if (collectionAuditList.Count() == 0)
            {
                return Enumerable.Empty<HistoryModel>();
            }

            var groupedAuditList = collectionAuditList.GroupBy(x => x.OperationId).OrderBy(x => x.Min(y => y.InsertedOn.Ticks)).ToList();

            var historyList = new List<HistoryModel>();
            var groupsCount = groupedAuditList.Count();

            if (groupedAuditList[0].Min(x => x.CreatedOn) >= fm.DateFrom && groupedAuditList[0].Min(x => x.CreatedOn) <= fm.DateTo)
            {
                var insertHistoryModel = new HistoryModel
                {
                    UserId = groupedAuditList[0].First().CreatedById,
                    DateTime = groupedAuditList[0].First().CreatedOn,
                    Field = entityFields.First(),
                    PreviousValue = string.Empty,
                    NewValue = string.Join(", ", groupedAuditList[0].Select(x => typeof(T).GetProperty(entityFields.First()).GetValue(x).ToString())),
                };
                historyList.Add(insertHistoryModel);
            }

            if (groupsCount == 1)
            {
                return historyList;
            }

            for (int i = 0; i < groupsCount - 1; i++)
            {
                var previousValue = string.Join(", ", groupedAuditList[i].Select(x => typeof(T).GetProperty(entityFields.First()).GetValue(x).ToString()));
                var newValue = string.Join(", ", groupedAuditList[i + 1].Select(x => typeof(T).GetProperty(entityFields.First()).GetValue(x).ToString()));

                var isDifferentOperations = groupedAuditList[i + 1].Min(x => x.CreatedOn) > groupedAuditList[i].Min(x => x.InsertedOn).AddSeconds(1);
                var isConcurrentTransaction = groupedAuditList[i + 1].Min(x => x.CreatedOn) <= groupedAuditList[i].Min(x => x.InsertedOn).AddSeconds(1)
                    && groupedAuditList[i + 1].First().CreatedById != groupedAuditList[i].First().InsertedBy;

                if (string.Equals(newValue, previousValue, StringComparison.CurrentCultureIgnoreCase)
                    || isDifferentOperations || isConcurrentTransaction)
                {
                    var deleteHistoryModel = new HistoryModel
                    {
                        UserId = groupedAuditList[i].First().InsertedBy,
                        DateTime = groupedAuditList[i].First().InsertedOn,
                        Field = entityFields.First(),
                        PreviousValue = previousValue,
                        NewValue = string.Empty,
                    };
                    historyList.Add(deleteHistoryModel);

                    if (groupedAuditList[i + 1].First().CreatedOn <= fm.DateTo)
                    {
                        var insertHistoryModel = new HistoryModel
                        {
                            UserId = groupedAuditList[i + 1].First().CreatedById,
                            DateTime = groupedAuditList[i + 1].First().CreatedOn,
                            Field = entityFields.First(),
                            PreviousValue = string.Empty,
                            NewValue = newValue,
                        };
                        historyList.Add(insertHistoryModel);
                    }

                    continue;
                }

                var historyModel = new HistoryModel
                {
                    UserId = groupedAuditList[i].First().InsertedBy,
                    DateTime = groupedAuditList[i].First().InsertedOn,
                    Field = entityFields.First(),
                    PreviousValue = previousValue,
                    NewValue = newValue,
                };
                historyList.Add(historyModel);
            }

            return historyList;
        }

        private IEnumerable<HistoryModel> AddHistories<T>(IEnumerable<T> auditList, T entity, FilterModel fm, string[] entityFields)
            where T : DAL.Queries.Entities.History.IHistoryAuditModel<Guid>
        {
            if (auditList.Count() != 0)
            {
                if (entity.ModifiedOn > fm.DateFrom && entity.ModifiedOn < fm.DateTo)
                {
                    auditList = auditList.Concat(new[] { entity });
                }

                var result = auditList.OrderBy(@p => @p.ModifiedOn).ToArray();

                var list = new List<HistoryModel>();
                for (int i = 0; i < result.Count() - 1; i++)
                {
                    list.AddRange(this.Compare(result[i], result[i + 1], entityFields));
                }

                return list;
            }
            else
            {
                return Enumerable.Empty<HistoryModel>();
            }
        }

        private IEnumerable<HistoryModel> Compare<T>(T prOld, T prNew, string[] entityFields)
            where T : DAL.Queries.Entities.History.IHistoryAuditModel<Guid>
        {
            Type type = typeof(T);

            foreach (System.Reflection.PropertyInfo property in type.GetProperties().Where(p => entityFields.Contains(p.Name)).ToArray())
            {
                if ((property.GetValue(prOld) != null && property.GetValue(prNew) != null) && !property.GetValue(prOld, null).Equals(property.GetValue(prNew, null)))
                {
                    yield return new HistoryModel
                    {
                        UserId = prOld.InsertedBy,
                        DateTime = prNew.ModifiedOn,
                        Field = property.Name,
                        PreviousValue = type.GetProperty(property.Name).GetValue(prOld, null).ToString(),
                        NewValue = type.GetProperty(property.Name).GetValue(prNew, null).ToString(),
                    };
                }
                else if (property.GetValue(prOld) == null && property.GetValue(prNew) != null)
                {
                    yield return new HistoryModel
                    {
                        UserId = prOld.InsertedBy,
                        DateTime = prNew.ModifiedOn,
                        Field = property.Name,
                        PreviousValue = string.Empty,
                        NewValue = type.GetProperty(property.Name).GetValue(prNew, null).ToString(),
                    };
                }
                else if (property.GetValue(prOld) != null && property.GetValue(prNew) == null)
                {
                    yield return new HistoryModel
                    {
                        UserId = prOld.InsertedBy,
                        DateTime = prNew.ModifiedOn,
                        Field = property.Name,
                        PreviousValue = type.GetProperty(property.Name).GetValue(prOld).ToString(),
                        NewValue = string.Empty,
                    };
                }
            }
        }
    }
}
