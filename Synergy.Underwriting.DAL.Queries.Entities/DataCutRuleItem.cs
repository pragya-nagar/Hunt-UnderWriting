using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class DataCutRuleItem : IAuditEntity<Guid>
    {
        public string Value { get; set; }

        public Guid DataCutRuleId { get; set; }

        public DataCutRule DataCutRule { get; set; }

        public int DataCutLogicTypeId { get; set; }

        public DataCutLogicType DataCutLogicType { get; set; }

        public int DataCutRuleFieldId { get; set; }

        public DataCutRuleField DataCutRuleField { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
