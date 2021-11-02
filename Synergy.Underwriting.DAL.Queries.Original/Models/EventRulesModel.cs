using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class EventRulesModel : IModel
    {
        public Guid EventId { get; set; }

        public Guid RuleId { get; set; }

        public FastEntityModel<int> DataCutResultType { get; set; }

        public IEnumerable<DataCutRuleItemModel> DataCutRuleItems { get; set; }
    }
}
