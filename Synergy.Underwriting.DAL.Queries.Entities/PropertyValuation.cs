using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyValuationBase : IAuditEntity<Guid>
    {
        public Guid PropertyId { get; set; }

        public float? LandValue { get; set; }

        public float? ImprovementValue { get; set; }

        public float? AppraisedValue { get; set; }

        public int AppraisedYear { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }

    public class PropertyValuation : PropertyValuationBase
    {
    }
}
