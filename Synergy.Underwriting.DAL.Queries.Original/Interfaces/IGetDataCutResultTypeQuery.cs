using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetDataCutResultTypeQuery : IQuery<IGetDataCutResultTypeQuery, IEnumerable<FastEntityModel<int>>>
    {
    }
}
