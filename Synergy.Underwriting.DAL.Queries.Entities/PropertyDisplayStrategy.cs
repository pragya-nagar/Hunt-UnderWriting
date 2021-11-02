using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyDisplayStrategyBase : IEntity
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }

    public class PropertyDisplayStrategy : PropertyDisplayStrategyBase
    {
        public IEnumerable<DelinquencyPropertyDisplayStrategy> DelinquencyPropertyDisplayStrategy { get; set; }
    }
}
