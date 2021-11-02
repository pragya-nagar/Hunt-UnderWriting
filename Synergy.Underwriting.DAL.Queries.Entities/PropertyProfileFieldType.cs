using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyProfileFieldType : IAuditEntity<int>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<PropertyProfileLogicType> PropertyProfileLogicTypes { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public int Id { get; set; }
    }
}
