using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.EventAssignment;
using Synergy.Underwriting.Models.Rule;

namespace Synergy.Underwriting.Domain.Abstracts
{
    public interface IEventService
    {
        Task<SearchResultModel<EventModel>> GetListAsync(SearchArgsModel<EventFilterArgs, EventSortField> args, CancellationToken cancellationToken = default(CancellationToken));

        Task<EventDetailsModel> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<EventCalculatedFieldsModel> FindEventCalculatedData(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<ObjectAccessModel> FindAttachmentAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteAttachmentAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<EventAssignmentResult> GetLevelListAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<string>> GetDumpFieldsAsync(Guid eventId, CancellationToken cancellationToken = default(CancellationToken));

        Task SetLockStatusAsync(Guid id, CancellationToken cancellationToken);

        Task<SearchResultModel<RuleModel>> GetRuleListAsync(Guid id,  CancellationToken cancellationToken = default(CancellationToken));

        Task<FileId> CreateRulesDumpAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    }
}
