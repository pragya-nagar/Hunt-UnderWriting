using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Abstractions.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetCountyQuery : IQuery<IGetCountyQuery, IEnumerable<FastEntityModel<int>>>,
                                       IHavePagination<IGetCountyQuery>,
                                       IHaveSearch<IGetCountyQuery>
    {
        IGetCountyQuery FilterByState(int stateId);
    }
}
