using System;

namespace Synergy.Underwriting.Models.Property
{
    public class PropertyAttachmentModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid UserId { get; set; }

        public DateTime FileCreatedOn { get; set; }
    }
}
