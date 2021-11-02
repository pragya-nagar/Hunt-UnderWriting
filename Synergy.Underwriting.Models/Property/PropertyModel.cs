using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;
using Synergy.Underwriting.Models.Address;

namespace Synergy.Underwriting.Models.Property
{
    public class PropertyModel : IResultModel
    {
        public Guid Id { get; set; }

        public string ParcelId { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public int CountyId { get; set; }

        public LeadModel Lead { get; set; }

        public AddressModel Address { get; set; }

        public decimal AppraisedValue { get; set; }

        public decimal AmountDue { get; set; }

        public decimal Ltv { get; set; }

        public decimal RuLtv { get; set; }

        public decimal RuAmount { get; set; }

        public string LandUseCode { get; set; }

        public decimal LandValue { get; set; }

        public decimal ImprovementValue { get; set; }

        public FastEntityModel<int> CurrentDecision { get; set; }

        public IEnumerable<PropertyAttachmentModel> PropertyAttachments { get; set; }
    }
}
