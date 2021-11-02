using System;
using System.Collections.Generic;
using Synergy.Common.Domain.Models.Abstracts;
using Synergy.Common.Domain.Models.Common;

namespace Synergy.Underwriting.Models
{
    public class DepartmentUsersModel : IResultModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<FastEntityModel<Guid>> Users { get; set; }
    }
}
