using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetEventDataCutDecisionQuery : IQuery<IGetEventDataCutDecisionQuery, IEnumerable<EventDataCutDecisionModel>>
    {
        IGetEventDataCutDecisionQuery FindByEventId(Guid eventId);

        IGetEventDataCutDecisionQuery FindByIsActive(bool isActive);
    }
}
