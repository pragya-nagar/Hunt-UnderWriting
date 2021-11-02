using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class DelinquencyModel
    {
        public Guid Id { get; set; }

        public string ParcelId { get; set; }

        public string AdvertisementNumber { get; set; }

        public int? TaxYear { get; set; }

        public Guid? ResultId { get; set; }

        public DateTime? ResultCreatedOn { get; set; }

        public Guid? ResultCreatedById { get; set; }
}
}
