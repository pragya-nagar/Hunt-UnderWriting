using System;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class PropertyAttachmentModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid UserId { get; set; }

        public DateTime FileCreatedOn { get; set; }
    }
}
