using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetExportPropertiesQuery : CollectionQuery<Guid, ExportPropertyModel>
    {
        private readonly Guid _systemUserId = Common.Constants.User.SystemUserId;

        private readonly ISynergyContext _synergyContext;
        private readonly ILogger<GetExportPropertiesQuery> _logger;

        public GetExportPropertiesQuery(ISynergyContext synergyContext, ILogger<GetExportPropertiesQuery> logger)
        {
            this._synergyContext = synergyContext ?? throw new ArgumentNullException(nameof(synergyContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<IEnumerable<ExportPropertyModel>> ExecuteAsync(Guid eventId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            this._logger.LogInformation("Start loading property data.");

            var delinquency = this._synergyContext.Delinquency.Where(x => x.EventId == eventId && x.DeletedOn == null);

            var initialData = await delinquency.Select(x => new ExportPropertyModel
                {
                    DeliquencyId = x.Id,
                    LegalDescription = x.Property.LegalDescription,
                    DelinquencyYear = x.DelinquencyTaxYear,
                    YearBuilt = x.Property.YearBuilt,
                    AmountDue = x.Amount,
                    LandUseCode = x.Property.LandUseCode,
                    GeneralLandUseCode = x.Property.GeneralLandUseCode.Name,
                    InternalLandUseCode = x.Property.InternalLandUseCode.Description,
                    RUAmount = x.RUAmount == null ? 0 : x.RUAmount.Value,
                    LTV = x.LTVPercent == null ? 0 : x.LTVPercent.Value * 100,
                    Owner = x.Property.Lead.AccountName,
                    PropertyAddress = x.Property.Address,
                    PropertyCity = x.Property.City,
                    PropertyState = x.Property.State.Abbreviation,
                    PropertyStateId = x.Property.StateId,
                    PropertyZipCode = x.Property.ZipCode,
                    ParcelId = x.Property.ParcelId,
                    Homestead = x.Property.Homestead,
                    MailingAddress1 = x.Property.Lead.MailingAddress1,
                    MailingAddress2 = x.Property.Lead.MailingAddress2,
                    MailingAddress3 = x.Property.Lead.MailingAddress3,
                    MailingCity = x.Property.Lead.MailingCity,
                    MailingState = x.Property.Lead.MailingState.Abbreviation,
                    MailingZipCode = x.Property.Lead.MailingZipCode,
                    LandAcres = x.Property.LandAcres == null ? 0 : x.Property.LandAcres.Value,
                    BuildingSqFt = x.Property.BuildingSqFt == null ? 0 : x.Property.BuildingSqFt.Value,
                    Scoring = x.DelinquencyPropertyScorings.Select(y => y.PropertyScoring).FirstOrDefault(),
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("Initial property data loaded.");

            var states = initialData.Where(x => x.PropertyStateId != null).Select(x => x.PropertyStateId).Distinct().ToList();
            var taxes = await this._synergyContext.StateTaxe
                .Where(x => x.DeletedOn == null && states.Contains(x.StateId))
                .Select(x => new { x.StateId, x.TaxRate })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var propertyDisplayStrategies = await delinquency.Join(
                this._synergyContext.DelinquencyPropertyDisplayStrategy,
                x => x.Id,
                x => x.DelinquencyId,
                (d, s) => new
                {
                    DelinquencyId = d.Id,
                    DisplayStrategyName = s.PropertyDisplayStrategy.Description,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var propertyValuations = await delinquency
                .Join(this._synergyContext.PropertyValuation.Where(v => v.IsActive == true && v.DeletedOn == null),
                x => x.PropertyId,
                x => x.PropertyId,
                (d, val) => new
                {
                    DelinquencyId = d.Id,
                    PropertyValuation = val,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            this._logger.LogInformation("Property Valuations data loaded.");

            var propertySupplementalEventDatas = await delinquency
                .Join(this._synergyContext.PropertySupplementalEventData.Where(v => v.DeletedOn == null),
                x => x.Id,
                x => x.DelinquencyId,
                (d, val) => new
                {
                    DelinquencyId = d.Id,
                    PropertySupplementalEventData = val,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            this._logger.LogInformation("propertySupplementalEvent data loaded.");

            var results = await delinquency
                .Join(this._synergyContext.Result.Where(v => v.DeletedOn == null),
                x => x.Id,
                x => x.DelinquencyId,
                (d, val) => new
                {
                    DelinquencyId = d.Id,
                    Result = new
                    {
                        val.BidNumber,
                        val.CertNo,
                        val.TaxAmount,
                        val.Overbid,
                        val.Premium,
                        val.RecoverableFees,
                        val.NonRecoverableFees,
                        val.InterestRate,
                        val.PenaltyRate,
                        val.Bid,
                    },
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            this._logger.LogInformation("Results data loaded.");

            var autoDecisions = await delinquency
                .Join(this._synergyContext.EventDataCutDecision.Where(x => x.EventDataCutStrategy.IsActive == true && x.DeletedOn == null),
                x => x.Id,
                x => x.DelinquencyId,
                (d, val) => new
                {
                    DelinquencyId = d.Id,
                    DecisionType = val.DecisionType.Name,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            this._logger.LogInformation("EventDataCutDecision data loaded.");

            var manualDecisions = await delinquency
                .Join(this._synergyContext.Decision.Where(x => x.DeletedOn == null),
                    x => x.Id,
                    x => x.DelinquencyId,
                    (d, dv) => new
                    {
                        DelinquencyId = d.Id,
                        Decision = new
                        {
                            DecisionType = dv.DecisionType.Name,
                            Comment = dv.Comment,
                            UserName = dv.User.FirstName + " " + dv.User.LastName,
                            UserId = dv.User.Id,
                            IsFinal = dv.EventDecisionLevel.IsFinal,
                            Order = dv.EventDecisionLevel.Order,
                            Name = dv.EventDecisionLevel.Name,
                        },
                    })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            this._logger.LogInformation("Decision data loaded.");

            var mergedData = initialData
                .GroupJoin(propertyValuations,
                    x => x.DeliquencyId,
                    x => x.DelinquencyId,
                    (x, val) => (Deliquency: x,
                                 PropertyValuation: val.OrderByDescending(v => v.PropertyValuation.AppraisedYear).FirstOrDefault()?.PropertyValuation))

                .GroupJoin(propertySupplementalEventDatas,
                    x => x.Deliquency.DeliquencyId,
                    x => x.DelinquencyId,
                    (x, val) => (Deliquency: x.Deliquency,
                            PropertyValuation: x.PropertyValuation,
                            PropertySupplementalEventData: val.FirstOrDefault()?.PropertySupplementalEventData))

                .GroupJoin(results,
                    x => x.Deliquency.DeliquencyId,
                    x => x.DelinquencyId,
                    (x, val) => (Deliquency: x.Deliquency,
                            PropertyValuation: x.PropertyValuation,
                            PropertySupplementalEventData: x.PropertySupplementalEventData,
                            Result: val.FirstOrDefault()?.Result))

                .GroupJoin(autoDecisions,
                    x => x.Deliquency.DeliquencyId,
                    x => x.DelinquencyId,
                    (x, val) => (Deliquency: x.Deliquency,
                            PropertyValuation: x.PropertyValuation,
                            PropertySupplementalEventData: x.PropertySupplementalEventData,
                            Result: x.Result,
                            AutoDecision: val.FirstOrDefault()))

                .GroupJoin(manualDecisions,
                    x => x.Deliquency.DeliquencyId,
                    x => x.DelinquencyId,
                    (x, val) => (Deliquency: x.Deliquency,
                            PropertyValuation: x.PropertyValuation,
                            PropertySupplementalEventData: x.PropertySupplementalEventData,
                            Result: x.Result,
                            AutoDecision: x.AutoDecision,
                            ManualDecisions: val.Select(md => md.Decision)))

                .GroupJoin(propertyDisplayStrategies,
                    x => x.Deliquency.DeliquencyId,
                    x => x.DelinquencyId,
                    (x, val) => (Deliquency: x.Deliquency,
                            PropertyValuation: x.PropertyValuation,
                            PropertySupplementalEventData: x.PropertySupplementalEventData,
                            Result: x.Result,
                            AutoDecision: x.AutoDecision,
                            ManualDecisions: x.ManualDecisions,
                            DisplayStrategies: val.Select(s => s.DisplayStrategyName)))

                .GroupJoin(taxes,
                    x => x.Deliquency.PropertyStateId,
                    x => x.StateId,
                    (x, val) => (Deliquency: x.Deliquency,
                            PropertyValuation: x.PropertyValuation,
                            PropertySupplementalEventData: x.PropertySupplementalEventData,
                            Result: x.Result,
                            AutoDecision: x.AutoDecision,
                            ManualDecisions: x.ManualDecisions,
                            DisplayStrategies: x.DisplayStrategies,
                            TaxRate: val.FirstOrDefault()?.TaxRate));

            this._logger.LogInformation("Data merged in memory.");

            foreach (var t in mergedData)
            {
                var (exportModel, propertyValuation, propertySupplementalEventData, result, autoDecision, manualDecision, displayStrategies, taxRate) = t;

                exportModel.DisplayStrategies = displayStrategies;

                exportModel.AppraisedValue = propertyValuation?.AppraisedValue ?? 0;
                exportModel.LandValue = propertyValuation?.LandValue ?? 0;
                exportModel.BuildingValue = propertyValuation?.ImprovementValue ?? 0;

                exportModel.LastSaleDate = propertySupplementalEventData?.LastSaleDate;
                exportModel.LastSaleAmount = propertySupplementalEventData?.LastSaleAmount ?? 0;
                exportModel.Mortgage1 = new ExportMortgageModel
                {
                    Loan = propertySupplementalEventData?.MortgageLoanAmount1 ?? 0,
                    MaturityDate = propertySupplementalEventData?.MortgageMaturityDate1,
                };
                exportModel.Mortgage2 = new ExportMortgageModel
                {
                    Loan = propertySupplementalEventData?.MortgageLoanAmount2 ?? 0,
                    MaturityDate = propertySupplementalEventData?.MortgageMaturityDate2,
                };

                exportModel.OpenLiens = propertySupplementalEventData?.OpenLiens ?? 0;
                exportModel.ClosedLiens = propertySupplementalEventData?.ClosedLiens ?? 0;
                exportModel.RecentBuyerName = propertySupplementalEventData?.RecentBuyerName;
                exportModel.RecentBuyerRate = propertySupplementalEventData?.RecentBuyerRate ?? 0;
                exportModel.Comment = propertySupplementalEventData?.InspectorComment;
                exportModel.PropertyRating = propertySupplementalEventData?.InspectorPropertyRating ?? 0;
                exportModel.AreaRating = propertySupplementalEventData?.InspectorAreaRating ?? 0;
                exportModel.Occupied = propertySupplementalEventData?.InspectorOccupied;
                exportModel.RoofCondition = propertySupplementalEventData?.InspectorRoofCondition;
                exportModel.LawnMaintained = propertySupplementalEventData?.InspectorLawnMaintained;
                exportModel.AdvertisementBatch = propertySupplementalEventData?.AdvertisementBatch;
                exportModel.AdvertisementNumber = propertySupplementalEventData?.AdvertisementNumber;

                exportModel.BidNumber = result?.BidNumber;
                exportModel.CertNo = result?.CertNo;
                exportModel.TaxAmount = result?.TaxAmount ?? 0;
                exportModel.Overbid = result?.Overbid ?? 0;
                exportModel.Premium = result?.Premium ?? 0;
                exportModel.RecoverableFees = result?.RecoverableFees ?? 0;
                exportModel.NonRecoverableFees = result?.NonRecoverableFees ?? 0;
                exportModel.ResultInterestRate = result?.InterestRate ?? 0;
                exportModel.PenaltyRate = result?.PenaltyRate ?? 0;

                exportModel.PurchasingEntity = result?.Bid?.Entity;
                exportModel.Portfolio = result?.Bid?.Portfolio;
                exportModel.TotalPurchaseAmount = (result?.TaxAmount ?? 0) + (result?.Overbid ?? 0) + (result?.RecoverableFees ?? 0);

                if (taxRate != null)
                {
                    var rate = exportModel.AppraisedValue * taxRate;
                    exportModel.TaxRatio = rate > 0 ? exportModel.AmountDue / rate.Value * 100 : 0;
                }

                if (autoDecision != null)
                {
                    exportModel.FinalDecision = autoDecision.DecisionType;
                    exportModel.FinalReviewer = exportModel.FinalReason = string.Empty;
                    exportModel.CurrentDecision = autoDecision.DecisionType;
                    exportModel.CurrentDecisionLevel = "Data cut decision";
                }
                else
                {
                    var finalDecision = manualDecision?.FirstOrDefault(v => v.IsFinal == true && v.UserId != this._systemUserId);
                    if (finalDecision != null)
                    {
                        exportModel.FinalDecision = finalDecision.DecisionType;
                        exportModel.FinalReason = finalDecision.Comment;
                        exportModel.FinalReviewer = finalDecision.UserName;
                    }

                    var decision = manualDecision?
                        .Where(d => string.IsNullOrWhiteSpace(d.DecisionType) == false && d.UserId != this._systemUserId)
                        .OrderByDescending(x => x.Order)
                        .FirstOrDefault();

                    if (decision != null)
                    {
                        exportModel.CurrentDecision = decision.DecisionType;
                        exportModel.CurrentDecisionLevel = decision.Name;
                        exportModel.CurrentDecisionReason = decision.Comment;
                        exportModel.CurrentDecisionReviewer = decision.UserName;
                    }
                }

                if (manualDecision != null)
                {
                    exportModel.Level = manualDecision
                        .Where(d => d.IsFinal == false && d.UserId != this._systemUserId)
                        .OrderBy(d => d.Order)
                        .Select(d => new ExportLevelDumpModel
                        {
                            Decision = d.DecisionType,
                            Reason = d.Comment,
                            Reviewer = d.UserName,
                        });
                }
            }

            this._logger.LogInformation("Data dump model created.");

            return mergedData.Select(x => x.Deliquency).ToList();
        }
    }
}