﻿using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class EventTypeAudit : IAuditEntity<int>
    {
        public DateTime InsertedOn { get; set; }

        public Guid InsertedBy { get; set; }

        public Guid OperationId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
