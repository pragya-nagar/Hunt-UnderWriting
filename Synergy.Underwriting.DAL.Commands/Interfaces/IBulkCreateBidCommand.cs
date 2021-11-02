using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Interfaces
{
    public interface IBulkCreateBidCommand : ICommand<IEnumerable<CreateBidModel>>
    {
    }
}
