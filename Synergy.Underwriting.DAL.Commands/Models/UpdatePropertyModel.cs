using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class UpdatePropertyModel
    {
        /// <summary>
        /// Represents Delinquency id, based on this Id we can identify property Id and event Id.
        /// </summary>
        public Guid Id { get; set; }

        public bool? IsHomestead { get; set; }

        public int? GeneralLandUseCodeId { get; set; }

        public int? InternalLandUseCodeId { get; set; }

        public int? PropertyScoring { get; set; }

        public IEnumerable<int> DispStrategyIds { get; set; }
    }
}
