using System;

namespace Synergy.Underwriting.Models.Commands
{
    public class RuleItemModel
    {
        public Guid Id { get; set; }

        public string Value { get; set; }

        public int DataCutLogicTypeId { get; set; }

        public int DataCutRuleFieldId { get; set; }
    }
}