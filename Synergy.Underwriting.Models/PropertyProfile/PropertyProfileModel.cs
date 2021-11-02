using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models.PropertyProfile
{
    public class PropertyProfileModel : IResultModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<FastEntityModel<int>> States { get; set; }
    }
}