namespace Synergy.Underwriting.Models.Rule
{
    public class CreateRuleItemArgs
    {
        public string Value { get; set; }

        public int DataCutLogicTypeId { get; set; }

        public int DataCutRuleFieldId { get; set; }
    }
}
