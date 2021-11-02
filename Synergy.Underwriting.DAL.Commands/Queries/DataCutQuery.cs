using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class DataCutQuery : CollectionQuery<Guid, CreateEventDataCutDecisionModel>
    {
        private readonly ISynergyContext _context;

        public DataCutQuery(ISynergyContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override async Task<IEnumerable<CreateEventDataCutDecisionModel>> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var rules = await (from ruleItem in this._context.DataCutRuleItem
                               join rule in this._context.DataCutRule on ruleItem.DataCutRuleId equals rule.Id
                               join er in this._context.EventDataCutRule on rule.Id equals er.DataCutRuleId
                               join strategy in this._context.EventDataCutStrategy on er.EventDataCutStrategyId equals strategy.Id
                               where strategy.EventId == eventId && strategy.IsActive
                               group ruleItem by new { rule.Id, ResultTypeId = rule.DataCutResultTypeId, StrategyId = strategy.Id } into g
                               select new
                               {
                                   g.Key.Id,
                                   g.Key.StrategyId,
                                   ResultType = (DataCutResultType)g.Key.ResultTypeId,
                                   Items = from item in g
                                           select new
                                           {
                                               LogicType = (DataCutLogicType)item.DataCutLogicTypeId,
                                               RuleField = (DataCutRuleField)item.DataCutRuleFieldId,
                                               item.Value,
                                           },
                               }).ToArrayAsync(cancellationToken).ConfigureAwait(false);

            if (rules.Any() == false)
            {
                return Enumerable.Empty<CreateEventDataCutDecisionModel>();
            }

            var rootList = await (from delinquency in this._context.Delinquency
                                  join property in this._context.Property on delinquency.PropertyId equals property.Id
                                  join lead in this._context.Lead on property.LeadId equals lead.Id
                                  join supplemental in this._context.PropertySupplementalEventData on delinquency.Id equals supplemental.DelinquencyId
                                  join valuation in this._context.PropertyValuation.Where(x => x.IsActive) on property.Id equals valuation.PropertyId into left
                                  from valuation in left.DefaultIfEmpty()
                                  where delinquency.EventId == eventId
                                  select new
                                  {
                                      delinquency.Id,
                                      AccountName = lead.AccountName.ToLower(CultureInfo.InvariantCulture),
                                      Address = property.Address == null ? string.Empty : property.Address.ToLower(CultureInfo.InvariantCulture),
                                      ZipCode = property.ZipCode == null ? string.Empty : property.ZipCode.ToLower(CultureInfo.InvariantCulture),
                                      LandUseCode = property.LandUseCode.ToLower(CultureInfo.InvariantCulture),
                                      property.GeneralLandUseCodeId,
                                      delinquency.Amount,
                                      LTVPercent = delinquency.LTVPercent == null ? 0 : delinquency.LTVPercent * 100,
                                      RULTVPercent = delinquency.RULTVPercent == null ? 0 : delinquency.RULTVPercent * 100,
                                      OpenLiens = supplemental.OpenLiens ?? 0,
                                      ClosedLiens = supplemental.ClosedLiens ?? 0,
                                      AppraisedValue = valuation == null ? 0 : valuation.AppraisedValue,
                                      ImprovementValue = valuation == null ? 0 : valuation.ImprovementValue,
                                      LandValue = valuation == null ? 0 : valuation.LandValue,
                                  }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var taxRate = 0M;
            if (rules.Any(x => x.Items.Any(y => y.RuleField == DataCutRuleField.TaxRatioPercent)))
            {
                taxRate = await (from tax in this._context.StateTaxe
                                 join state in this._context.State on tax.StateId equals state.Id
                                 join @event in this._context.Event on state.Id equals @event.StateId
                                 where @event.Id == eventId
                                 select tax.TaxRate).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }

            var list = new List<CreateEventDataCutDecisionModel>();
            foreach (var rule in rules)
            {
                var aggregate = from item in rootList
                    select new
                    {
                        item.Id,
                        item.AccountName,
                        item.Address,
                        item.ZipCode,
                        item.LandUseCode,
                        item.GeneralLandUseCodeId,
                        item.Amount,
                        item.LTVPercent,
                        item.RULTVPercent,
                        item.OpenLiens,
                        item.ClosedLiens,
                        item.AppraisedValue,
                        item.ImprovementValue,
                        item.LandValue,
                        TaxRatioPercent = item.AppraisedValue * taxRate == 0 ? 0 : item.Amount / (item.AppraisedValue * taxRate) * 100,
                    };

                foreach (var item in rule.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.Value))
                    {
                        continue;
                    }

                    var value = item.Value.ToLower(CultureInfo.InvariantCulture);
                    switch (item.RuleField)
                    {
                        case DataCutRuleField.AccountName:
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.Contains:
                                    aggregate = aggregate.Where(p => p.AccountName.Contains(value) == true);

                                    break;
                                case DataCutLogicType.DoesNotContain:
                                    aggregate = aggregate.Where(p => p.AccountName.Contains(value) == false);

                                    break;
                                case DataCutLogicType.Equal:
                                    aggregate = aggregate.Where(p => p.AccountName == value);

                                    break;
                                case DataCutLogicType.NotEqual:
                                    aggregate = aggregate.Where(p => p.AccountName != value);

                                    break;
                            }

                            break;
                        case DataCutRuleField.PropertyAddress:
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.Contains:
                                    aggregate = aggregate.Where(p => p.Address.Contains(value) == true);

                                    break;
                                case DataCutLogicType.DoesNotContain:
                                    aggregate = aggregate.Where(p => p.Address.Contains(value) == false);

                                    break;
                                case DataCutLogicType.Equal:
                                    aggregate = aggregate.Where(p => p.Address == value);

                                    break;
                                case DataCutLogicType.NotEqual:
                                    aggregate = aggregate.Where(p => p.Address != value);

                                    break;
                            }

                            break;
                        case DataCutRuleField.PropertyZipCode:
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.Contains:
                                    aggregate = aggregate.Where(p => p.ZipCode.Contains(value) == true);

                                    break;
                                case DataCutLogicType.DoesNotContain:
                                    aggregate = aggregate.Where(p => p.ZipCode.Contains(value) == false);

                                    break;
                                case DataCutLogicType.Equal:
                                    aggregate = aggregate.Where(p => p.ZipCode == value);

                                    break;
                                case DataCutLogicType.NotEqual:
                                    aggregate = aggregate.Where(p => p.ZipCode != value);

                                    break;
                            }

                            break;
                        case DataCutRuleField.LandUseCode:
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.Contains:
                                    aggregate = aggregate.Where(p => p.LandUseCode.Contains(value) == true);

                                    break;
                                case DataCutLogicType.DoesNotContain:
                                    aggregate = aggregate.Where(p => p.LandUseCode.Contains(value) == false);

                                    break;
                                case DataCutLogicType.Equal:
                                    aggregate = aggregate.Where(p => p.LandUseCode == value);

                                    break;
                                case DataCutLogicType.NotEqual:
                                    aggregate = aggregate.Where(p => p.LandUseCode != value);

                                    break;
                            }

                            break;
                        case DataCutRuleField.GeneralLandUseCode when Enum.TryParse(item.Value, out GeneralLandUseCode generalLandUseCodeValue):
                            var generalLandUseCodeId = (int)generalLandUseCodeValue;
                            aggregate = aggregate.Where(p => p.GeneralLandUseCodeId == generalLandUseCodeId);

                            break;
                        case DataCutRuleField.LandValue when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.LandValue < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.LandValue <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.LandValue > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.LandValue >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.ImprovementValue when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.ImprovementValue < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.ImprovementValue <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.ImprovementValue > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.ImprovementValue >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.AppraisedValue when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.AppraisedValue < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.AppraisedValue <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.AppraisedValue > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.AppraisedValue >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.AmountDue when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.Amount < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.Amount <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.Amount > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.Amount >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.OpenLiens when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.OpenLiens < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.OpenLiens <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.OpenLiens > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.OpenLiens >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.ClosedLiens when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.ClosedLiens < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.ClosedLiens <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.ClosedLiens > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.ClosedLiens >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.LTVPercent when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.LTVPercent < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.LTVPercent <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.LTVPercent > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.LTVPercent >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.HorizonLTVPercent when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    break;
                                case DataCutLogicType.GreaterThan:
                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    break;
                            }

                            break;
                        case DataCutRuleField.RULTVPercent when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.RULTVPercent < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.RULTVPercent <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.RULTVPercent > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.RULTVPercent >= val);

                                    break;
                            }

                            break;
                        case DataCutRuleField.TaxRatioPercent when decimal.TryParse(value, out var val):
                            switch (item.LogicType)
                            {
                                case DataCutLogicType.LessThan:
                                    aggregate = aggregate.Where(p => p.TaxRatioPercent < val);

                                    break;
                                case DataCutLogicType.LessThanOrEqual:
                                    aggregate = aggregate.Where(p => p.TaxRatioPercent <= val);

                                    break;
                                case DataCutLogicType.GreaterThan:
                                    aggregate = aggregate.Where(p => p.TaxRatioPercent > val);

                                    break;
                                case DataCutLogicType.GreaterThanOrEqual:
                                    aggregate = aggregate.Where(p => p.TaxRatioPercent >= val);

                                    break;
                            }

                            break;
                    }
                }

                list = list.Union(aggregate.Select(x => new CreateEventDataCutDecisionModel
                {
                    Id = Guid.NewGuid(),
                    DelinquencyId = x.Id,
                    EventDataCutStrategyId = rule.StrategyId,
                    DecisionTypeId = rule.ResultType == DataCutResultType.DataApprove ? (int)DecisionType.AutoApprove : (int)DecisionType.AutoReject,
                })).ToList();
            }

            list = list.GroupBy(x => x.DelinquencyId).Select(x => new CreateEventDataCutDecisionModel()
            {
                Id = x.First().Id,
                EventDataCutStrategyId = x.First().EventDataCutStrategyId,
                DelinquencyId = x.Key,
                DecisionTypeId = x.Any(d => d.DecisionTypeId == (int)DecisionType.AutoReject) ? (int)DecisionType.AutoReject : (int)DecisionType.AutoApprove,
            }).ToList();

            return list;
        }
    }
}
