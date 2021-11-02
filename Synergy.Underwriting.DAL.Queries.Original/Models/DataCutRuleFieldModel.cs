using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class DataCutRuleFieldModel : IModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<DataCutLogicTypeModel> LogicTypes { get; set; }
    }
}
