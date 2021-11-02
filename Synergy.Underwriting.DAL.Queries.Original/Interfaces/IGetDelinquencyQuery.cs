using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetDelinquencyQuery : IQuery<IGetDelinquencyQuery, IEnumerable<DelinquencyModel>>,
                                            IHavePagination<IGetDelinquencyQuery>
    {
        IGetDelinquencyQuery FilterByPropertyIds(List<Guid> propertyIds);

        IGetDelinquencyQuery FilterByEventIds(List<Guid> eventIds);
    }
}
