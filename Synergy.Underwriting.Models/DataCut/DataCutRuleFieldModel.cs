using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models
{
    public class DataCutRuleFieldModel : IResultModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<DataCutLogicTypeModel> LogicTypes { get; set; }
    }
}