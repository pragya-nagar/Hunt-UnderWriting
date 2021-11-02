using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyProfileRulePropertyProfile : IAuditEntity<Guid>
    {
        public Guid PropertyProfileId { get; set; }

        public Guid PropertyProfileRuleId { get; set; }

        public PropertyProfile PropertyProfile { get; set; }

        public PropertyProfileRule PropertyProfileRule { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public Guid Id { get; set; }
    }
}
