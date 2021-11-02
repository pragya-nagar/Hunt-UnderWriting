using System.Collections.Generic;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    internal enum DataCutLogicType
    {
        Contains = 1,
        DoesNotContain = 2,
        Equal = 3,
        NotEqual = 4,
        LessThan = 5,
        LessThanOrEqual = 6,
        GreaterThan = 7,
        GreaterThanOrEqual = 8,
    }

    internal enum DataCutRuleField
    {
        AccountName = 1,
        PropertyAddress,
        PropertyZipCode,
        LandUseCode,
        GeneralLandUseCode,
        LandValue,
        ImprovementValue,
        AppraisedValue,
        AmountDue,
        OpenLiens,
        ClosedLiens,
        LTVPercent,
        HorizonLTVPercent,
        RULTVPercent,
        TaxRatioPercent,
    }

    internal enum DataCutResultType
    {
        DataReject = 1,
        DataApprove = 2,
    }
}
