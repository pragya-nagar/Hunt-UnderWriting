using System;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class DelinquencyModel : AuditModel, IModel
    {
        public Guid Id { get; set; }

        public int DelinquencyTaxYear { get; set; }

        public Guid PropertyId { get; set; }
    }
}
