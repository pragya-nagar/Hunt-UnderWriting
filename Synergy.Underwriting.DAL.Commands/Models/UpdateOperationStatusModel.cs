using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class UpdateOperationStatusModel
    {
        public Guid Id { get; set; }

        public int Code { get; set; }

        public int? CategoryId { get; set; }

        public int? SessionId { get; set; }

        public string Message { get; set; }

        public int? Progress { get; set; }
    }
}
