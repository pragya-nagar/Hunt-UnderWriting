using System;
using Synergy.Common.DAL.Abstract;

namespace Synergy.Underwriting.DAL.Queries.Entities
{
    public class PropertyAttachment : IAuditEntity<Guid>
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public string Path { get; set; }

        public DateTime FileCreatedOn { get; set; }

        public int TypeId { get; set; }

        public PropertyAttachmentType Type { get; set; }

        public Guid PropertyId { get; set; }

        public Property Property { get; set; }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
