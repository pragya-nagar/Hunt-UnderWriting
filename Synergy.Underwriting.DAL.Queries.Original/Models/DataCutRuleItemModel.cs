using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class DataCutRuleItemModel : AuditModel
    {
        public string Value { get; set; }

        public FastEntityModel<int> DataCutLogicType { get; set; }

        public FastEntityModel<int> DataCutRuleField { get; set; }
    }
}
