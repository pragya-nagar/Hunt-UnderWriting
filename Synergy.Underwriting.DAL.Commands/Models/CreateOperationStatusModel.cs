using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreateOperationStatusModel
    {
        public Guid Id { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public int? Progress { get; set; }
    }
}
