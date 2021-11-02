using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileRuleFieldModel : IResultModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public FastEntityModel<int> PropertyProfileFieldType { get; set; }

        public IEnumerable<FastEntityModel<int>> PropertyProfileLogicTypes { get; set; }
    }
}