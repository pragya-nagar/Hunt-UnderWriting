using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class LeadAddressModel
    {
        public FastEntityModel<int> State { get; set; }

        public string City { get; set; }

        public string Zip { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }
    }
}
