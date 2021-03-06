using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetDataCutRuleQuery : IQuery<IGetDataCutRuleQuery, IEnumerable<DataCutRuleModel>>
    {
        IGetDataCutRuleQuery FindByCounty(int countyId);
    }
}
