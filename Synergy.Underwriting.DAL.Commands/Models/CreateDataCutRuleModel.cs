using System;
using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class CreateDataCutRuleModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int CountyId { get; set; }

        public int DataCutResultTypeId { get; set; }

        public IEnumerable<DataCutRuleItemModel> DataCutRuleItems { get; set; }
    }
}
