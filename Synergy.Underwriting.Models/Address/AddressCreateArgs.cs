namespace Synergy.Underwriting.Models.Address
{
    public class AddressCreateArgs
    {
        public int? StateId { get; set; }

        public string City { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string Zip { get; set; }
    }
}
