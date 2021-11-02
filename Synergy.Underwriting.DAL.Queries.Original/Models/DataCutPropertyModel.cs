using System;
using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class DataCutPropertyModel : IModel
    {
        public Guid Id { get; set; }

        public Guid PropertyId { get; set; }

        /// <summary>
        /// Lead name
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Property data
        /// </summary>
        public string PropertyAddress { get; set; }

        public string PropertyZipCode { get; set; }

        public string LandUseCode { get; set; }

        public string InternalLandUseCode { get; set; }

        public string GeneralLandUseCode { get; set; }

        public decimal? RUAmount { get; set; }

        public decimal? RULTVPercent { get; set; }

        public decimal? LTVPercent { get; set; }

        /// <summary>
        /// Valuation data
        /// </summary>
        public decimal? ImprovementValue { get; set; }

        public decimal? AppraisedValue { get; set; }

        public decimal? LandValue { get; set; }

        /// <summary>
        /// PropertySupplementalEventDataBase data
        /// </summary>
        public int? OpenLiens { get; set; }

        public int? ClosedLiens { get; set; }

        /// <summary>
        /// State information
        /// </summary>
        public int StateId { get; set; }
    }
}
