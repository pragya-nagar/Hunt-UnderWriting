using Synergy.DataAccess.Abstractions.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Interfaces
{
    public interface IGetEventAttachmentQuery : ISingleQuery<IGetEventAttachmentQuery, EventAttachmentModel>
    {
    }
}
