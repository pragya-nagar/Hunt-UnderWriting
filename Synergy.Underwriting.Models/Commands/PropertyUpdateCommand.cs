using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.Underwriting.Models.Commands
{
    public class PropertyUpdateCommand : Command
    {
        public bool? IsHomestead { get; set; }

        public int? GeneralLandUseCodeId { get; set; }

        public int? InternalLandUseCodeId { get; set; }

        public int? Scoring { get; set; }

        public IEnumerable<int> DisplayStrategyIds { get; set; }
    }
}
