using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class DataCutRuleItemModel
    {
        public Guid Id { get; set; }

        public string Value { get; set; }

        public int DataCutLogicTypeId { get; set; }

        public int DataCutRuleFieldId { get; set; }
    }
}