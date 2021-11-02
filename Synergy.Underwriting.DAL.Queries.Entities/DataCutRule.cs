using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class DataCutRule : IAuditEntity<Guid>
    {
        public string Name { get; set; }

        public int CountyId { get; set; }

        public int DataCutResultTypeId { get; set; }

        public DataCutResultType DataCutResultType { get; set; }

        public IEnumerable<EventDataCutRule> EventRuleLinks { get; set; }

        public IEnumerable<DataCutRuleItem> DataCutRuleItems { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
