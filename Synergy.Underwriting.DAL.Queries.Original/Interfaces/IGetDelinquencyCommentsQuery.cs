using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetDelinquencyCommentsQuery : IQuery<IGetDelinquencyCommentsQuery, IEnumerable<DelinquencyCommentModel>>,
                                         IHavePagination<IGetDelinquencyCommentsQuery>
    {
        IGetDelinquencyCommentsQuery FilterByDelinquencies(IEnumerable<Guid> delinquencyIds);
    }
}
