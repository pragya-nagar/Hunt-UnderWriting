using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetDataCutPropertyQuery : IQuery<IGetDataCutPropertyQuery, IEnumerable<DataCutPropertyModel>>
    {
    }
}
