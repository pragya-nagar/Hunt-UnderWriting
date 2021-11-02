using System;
using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventCalculatedFieldsModel : IModel
    {
        public Guid? EventId { get; set; }

        public int? PreLimListCount { get; set; }

        public decimal? PreLimListAmount { get; set; }

        public int? ApprovedLienCount { get; set; }

        public decimal? ApprovedPurchaseAmount { get; set; }

        public int? FinalPurchaseCount { get; set; }

        public decimal? FinalPurchaseAmount { get; set; }
    }
}
