using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Queries
{
    public class GetEventCalculatedFields : BaseQuery<Delinquency>, IGetEventCalculatedFields
    {
        private DbSet<Delinquency> _entity;
        private ISynergyContext _context;
        private Guid _eventId;

        public GetEventCalculatedFields(ISynergyContext context)
        {
            _entity = context.Delinquency;
            _context = context;
        }

        public IGetEventCalculatedFields FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IGetEventCalculatedFields FilterByEventId(Guid eventId)
        {
            _eventId = eventId;
            andAlsoPredicates.Add(x => x.EventId == eventId);
            return this;
        }

        public IEnumerable<EventCalculatedFieldsModel> Exeсute()
        {
            var approvedLien = GetApproved().FirstOrDefault();

            var preLimList = GetExceptAutoRejected().FirstOrDefault();

            var finalPurchase = GetFinalPurchase();

            var eventCalculatedFieldsModel = new EventCalculatedFieldsModel();

            eventCalculatedFieldsModel.EventId = _eventId;
            eventCalculatedFieldsModel.PreLimListAmount = preLimList?.PreLimListAmount ?? 0;
            eventCalculatedFieldsModel.PreLimListCount = preLimList?.PreLimListCount ?? 0;
            eventCalculatedFieldsModel.ApprovedPurchaseAmount = approvedLien?.ApprovedPurchaseAmount ?? 0;
            eventCalculatedFieldsModel.ApprovedLienCount = approvedLien?.ApprovedLienCount ?? 0;
            eventCalculatedFieldsModel.FinalPurchaseAmount = finalPurchase?.FinalPurchaseAmount ?? 0;
            eventCalculatedFieldsModel.FinalPurchaseCount = finalPurchase?.FinalPurchaseCount ?? 0;

            return new List<EventCalculatedFieldsModel>
            {
                eventCalculatedFieldsModel,
            };
        }

        public async Task<IEnumerable<EventCalculatedFieldsModel>> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var approved = await GetApproved().FirstOrDefaultAsync().ConfigureAwait(false);

            var exceptAutoRejected = await GetExceptAutoRejected().FirstOrDefaultAsync().ConfigureAwait(false);

            var finalPurchase = GetFinalPurchase();

            var eventCalculatedFieldsModel = new EventCalculatedFieldsModel();

            eventCalculatedFieldsModel.EventId = _eventId;
            eventCalculatedFieldsModel.PreLimListAmount = exceptAutoRejected?.PreLimListAmount ?? 0;
            eventCalculatedFieldsModel.PreLimListCount = exceptAutoRejected?.PreLimListCount ?? 0;
            eventCalculatedFieldsModel.ApprovedPurchaseAmount = approved?.ApprovedPurchaseAmount ?? 0;
            eventCalculatedFieldsModel.ApprovedLienCount = approved?.ApprovedLienCount ?? 0;
            eventCalculatedFieldsModel.FinalPurchaseAmount = finalPurchase?.FinalPurchaseAmount ?? 0;
            eventCalculatedFieldsModel.FinalPurchaseCount = finalPurchase?.FinalPurchaseCount ?? 0;

            return new List<EventCalculatedFieldsModel>
            {
                eventCalculatedFieldsModel,
            };
        }

        private EventCalculatedFieldsModel GetFinalPurchase()
        {
            var delinqvencies = _entity.Where(GetPredicate()).Where(x => x.Results.Any(b => b.BidId != null)).ToList();
            var delinqvenciesIds = delinqvencies.Select(x => x.Id).ToList();

            var results = _context.Result.Where(x => delinqvenciesIds.Contains(x.DelinquencyId)).ToList();

            int finalPurchaseCount = 0;
            decimal finalPurchaseAmount = 0;

            foreach (var item in delinqvencies)
            {
                var result = results.Where(x => x.DelinquencyId == item.Id).FirstOrDefault();
                if (result == null)
                {
                    continue;
                }

                finalPurchaseCount += 1;
                finalPurchaseAmount += result.TaxAmount + (result.Overbid ?? 0) + (result.RecoverableFees ?? 0);
            }

            return new EventCalculatedFieldsModel
            {
                FinalPurchaseAmount = finalPurchaseAmount,
                FinalPurchaseCount = finalPurchaseCount,
            };
        }

        private IQueryable<EventCalculatedFieldsModel> GetExceptAutoRejected()
        {
            var preLimList = _entity.Where(GetPredicate())
                 .Where(x => !x.EventDataCutDecisions.Any(a => a.DecisionTypeId == (int)DataAccess.Enum.DecisionType.AutoReject && a.EventDataCutStrategy.IsActive))
                 .GroupBy(x => x.EventId)
                 .Select(group => new EventCalculatedFieldsModel
                 {
                     EventId = group.Key,
                     PreLimListCount = group.Count(),
                     PreLimListAmount = group.Sum(x => x.Amount),
                 });
            return preLimList;
        }

        private IQueryable<EventCalculatedFieldsModel> GetApproved()
        {
            var approvedLien = _entity.Where(GetPredicate())
                   .Where(x => x.EventDataCutDecisions.Any(a => a.DecisionTypeId == (int)DataAccess.Enum.DecisionType.AutoApprove && a.EventDataCutStrategy.IsActive) ||
                            (x.Decisions.Any(d => d.DecisionTypeId == (int)DataAccess.Enum.DecisionType.Approve && d.EventDecisionLevel.IsFinal) &&
                            !x.EventDataCutDecisions.Any(e => e.EventDataCutStrategy.IsActive)))
                   .GroupBy(x => x.EventId)
                   .Select(group => new EventCalculatedFieldsModel
                   {
                       EventId = group.Key,
                       ApprovedLienCount = group.Count(),
                       ApprovedPurchaseAmount = group.Sum(x => x.Amount),
                   });
            return approvedLien;
        }
    }
}
