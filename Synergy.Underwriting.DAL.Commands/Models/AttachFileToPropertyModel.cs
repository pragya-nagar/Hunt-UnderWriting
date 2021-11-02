using System;
using Synergy.DataAccess.Enum;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class AttachFileToPropertyModel
    {
        public Guid Id { get; set; }

        public Guid DelinquencyId { get; set; }

        public PropertyAttachmentType AttachmentType { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }
    }
}
