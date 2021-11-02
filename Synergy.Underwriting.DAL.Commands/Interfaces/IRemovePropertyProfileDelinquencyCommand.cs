using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;

namespace Synergy.Underwriting.DAL.Commands.Interfaces
{
    public interface IRemovePropertyProfileDelinquencyCommand : ICommand<IEnumerable<Guid>>
    {
    }
}
