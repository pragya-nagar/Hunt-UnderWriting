using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.Domain.Models.Common;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Property;

namespace Synergy.Underwriting.Domain.Abstracts
{
    public interface IPropertyService
    {
        Task<SearchResultModel<PropertyAssignmentModel>> GetListAsync(SearchArgsModel<PropertyFilterArgs, PropertySortField> args, CancellationToken cancellationToken = default);

        Task<SearchResultModel<DecisionLevelModel>> GetLevelListAsync(SearchArgsModel<DecisionLevelFilterArgs, DecisionLevelSortField> args, CancellationToken cancellationToken = default);

        Task<IEnumerable<ObjectAccessModel>> GetPhotoUrlListAsync(Guid delinquencyId, CancellationToken cancellationToken = default);

        Task<ObjectAccessModel> FindAttachmentAsync(Guid id, CancellationToken cancellationToken = default);

        Task DeleteAttachmentAsync(Guid id, CancellationToken cancellationToken = default);

        Task<SearchResultModel<DelinquencyCommentModel>> GetCommentsListAsync(SearchArgsModel<Guid, CommentSortField> args, CancellationToken cancellationToken = default);

        Task<DelinquencyCommentModel> GetCommentAsync(Guid delinquencyId, Guid commentId, CancellationToken cancellationToken = default);

        Task<SearchResultModel<DelinquencyHistoryModel>> GetDecisionHistoryAsync(SearchArgsModel<Guid, int> args, CancellationToken cancellationToken = default);

        Task<PropertyAssignmentModel> GetSnapshotAsync(Guid delinquencyId, DateTime? date, CancellationToken cancellationToken = default);
    }
}
