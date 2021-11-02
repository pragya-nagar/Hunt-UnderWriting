using System.Collections.Generic;

namespace Synergy.Underwriting.Models.Property
{
    public class PropertyUpdateArgs
    {
        public bool? IsHomestead { get; set; }

        public int? GeneralLandUseCodeId { get; set; }

        public int? InternalLandUseCodeId { get; set; }

        public int? Scoring { get; set; }

        public IEnumerable<int> DisplayStrategyIds { get; set; }
    }
}
