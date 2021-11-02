using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results.MailMerge
{
    public class MailMergeTemplateModel
    {
        public Guid Id { get; set; }

        public int GroupingType { get; set; }

        public string FileId { get; set; }
    }
}
