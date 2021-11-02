using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;
using Synergy.Underwriting.Models.DataCut;

namespace Synergy.Underwriting.Models.Commands
{
    public class DataCutCreateCommand : Command
    {
        public Guid EventId { get; set; }

        public IEnumerable<DataCutItem> DataCutItem { get; set; }
    }
}
