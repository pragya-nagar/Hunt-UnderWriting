using System;
using System.Collections.Generic;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyProfile : IAuditEntity<Guid>
    {
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<PropertyProfileDelinquency> PropertyProfileDelinquencies { get; set; }

        public IEnumerable<PropertyProfileState> PropertyProfileStates { get; set; }

        public IEnumerable<PropertyProfileRulePropertyProfile> PropertyProfileRulePropertyProfiles { get; set; }

        public IEnumerable<EventDecisionLevelPropertyProfile> EventDecisionLevelPropertyProfile { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }

        public Guid Id { get; set; }
    }
}
