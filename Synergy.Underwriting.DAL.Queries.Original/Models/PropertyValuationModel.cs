using System;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class PropertyValuationModel : AuditModel, IModel
    {
        public Guid Id { get; set; }

        public int AppraisedYear { get; set; }

        public decimal LandValue { get; set; }

        public decimal ImprovementValue { get; set; }

        public decimal AppraisedValue { get; set; }
    }
}
