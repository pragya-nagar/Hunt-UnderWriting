using System.Collections.Generic;

namespace Synergy.Underwriting.Models
{
    public class EventDictionaryFilterArgs
    {
        public string County { get; set; }

        public bool? IsLocked { get; set; }

        public IEnumerable<int> StateIds { get; set; }
    }
}