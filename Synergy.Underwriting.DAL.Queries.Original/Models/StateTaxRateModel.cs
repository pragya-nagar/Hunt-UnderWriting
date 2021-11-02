using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class StateTaxRateModel : AuditModel, IModel
    {
        public int Id { get; set; }

        public FastEntityModel<int> State { get; set; }

        public decimal TaxRate { get; set; }
    }
}
