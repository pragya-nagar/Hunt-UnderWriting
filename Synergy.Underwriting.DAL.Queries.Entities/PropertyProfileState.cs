using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyProfileState : IAuditEntity<Guid>
    {
        public Guid PropertyProfileId { get; set; }

        public int StateId { get; set; }

        public PropertyProfile PropertyProfile { get; set; }

        public State State { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public Guid Id { get; set; }
    }
}
