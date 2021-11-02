using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyProfileRule : IAuditEntity<Guid>
    {
        public string Name { get; set; }

        public IEnumerable<PropertyProfileRuleItem> PropertyProfileRuleItems { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public Guid Id { get; set; }
    }
}
