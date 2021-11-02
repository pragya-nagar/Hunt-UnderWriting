using System;
using System.Collections;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class State : IAuditEntity<int>
    {
        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public IEnumerable<LeadAudit> LeadAudit { get; set; }

        public IEnumerable<Lead> Lead { get; set; }
    }
}
