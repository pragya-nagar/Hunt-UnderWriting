using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models.Assigment;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetEventAssigmentsCountQuery : ISingleQuery<IGetEventAssigmentsCountQuery, EventAssignmentModel>
    {
    }
}
