using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class DataCutRuleModel : AuditModel, IModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public FastEntityModel<int> County { get; set; }

        public FastEntityModel<int> DataCutResultType { get; set; }

        public IEnumerable<DataCutRuleItemModel> DataCutRuleItems { get; set; }
    }
}
