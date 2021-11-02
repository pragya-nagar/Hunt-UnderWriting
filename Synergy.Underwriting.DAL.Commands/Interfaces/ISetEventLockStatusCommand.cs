using System;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;

namespace Synergy.Underwriting.DAL.Commands.Interfaces
{
    public interface ISetEventLockStatusCommand : ICommand<Guid>
    {
    }
}
