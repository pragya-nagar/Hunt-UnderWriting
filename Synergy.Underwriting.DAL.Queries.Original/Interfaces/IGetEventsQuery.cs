using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetEventsQuery : IQuery<IGetEventsQuery, IEnumerable<EventModel>>,
                                        IHavePagination<IGetEventsQuery>,
                                        IHaveUser<IGetEventsQuery>,
                                        IHaveSorting<IGetEventsQuery, EventSortField>,
                                        IHaveSearch<IGetEventsQuery>
    {
        IGetEventsQuery IncludeAttachments();

        IGetEventsQuery FilterByEventType(int eventTypeId);

        IGetEventsQuery FilterByState(int stateId);

        IGetEventsQuery FilterByStatus(bool isLocked);
    }
}
