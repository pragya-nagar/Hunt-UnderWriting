using System;

namespace Synergy.Underwriting.DAL.Commands.Models.Results
{
    public class BidModel
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public string Entity { get; set; }

        public string Portfolio { get; set; }

        public Guid EventId { get; set; }
    }
}