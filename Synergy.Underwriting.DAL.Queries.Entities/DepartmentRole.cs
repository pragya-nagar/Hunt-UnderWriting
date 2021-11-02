using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class DepartmentRole : IAuditEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid RoleId { get; set; }

        public int DepartmentId { get; set; }

        public bool IsManager { get; set; }

        public Department Department { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
