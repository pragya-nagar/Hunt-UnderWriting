using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class AttachFileToEventModel
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public string Path { get; set; }
    }
}
