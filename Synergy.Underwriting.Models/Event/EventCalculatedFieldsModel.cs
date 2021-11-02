using System;

namespace Synergy.Underwriting.Models
{
    public class EventCalculatedFieldsModel
    {
        public Guid Id { get; set; }

        public int? PreLimListCount { get; set; }

        public decimal? PreLimListAmount { get; set; }

        public int? ApprovedLienCount { get; set; }

        public decimal? ApprovedPurchaseAmount { get; set; }

        public int? FinalPurchaseCount { get; set; }

        public decimal? FinalPurchaseAmount { get; set; }
    }
}