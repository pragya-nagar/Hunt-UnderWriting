using System;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventAttachmentModel : AuditModel, IModel
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public string Path { get; set; }

        public byte[] Data { get; set; }
    }
}
