using System;
using System.Collections.Generic;
using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetEventDelinquenciesQuery : IQuery<IGetEventDelinquenciesQuery, IEnumerable<PropertyAssignmentModel>>,
                                                           IHavePagination<IGetEventDelinquenciesQuery>,
                                                           IHaveUser<IGetEventDelinquenciesQuery>,
                                                           IHaveSearch<IGetEventDelinquenciesQuery>
    {
        IGetEventDelinquenciesQuery FilterByEventIds(List<Guid> eventIds);

        IGetEventDelinquenciesQuery IncludeValuation();

        IGetEventDelinquenciesQuery IncludeLead();

        IGetEventDelinquenciesQuery IncludeSupplementalEventData();

        IGetEventDelinquenciesQuery FilterByDecisionType(ReviewStatusFilters status, Guid levelId, Guid userId);

        IGetEventDelinquenciesQuery FilterByPriorDecision(ReviewDecisionSearchField decisionType, Guid userId);

        IGetEventDelinquenciesQuery FilterByDelinquencyId(Guid delinquencyId);

        IGetEventDelinquenciesQuery SetOrder(bool moveForward);

        IGetEventDelinquenciesQuery FilterByInactiveDataCut();

        IGetEventDelinquenciesQuery FilterByPropertyFields(PropertyFieldsFilterModel fields);
    }
}
