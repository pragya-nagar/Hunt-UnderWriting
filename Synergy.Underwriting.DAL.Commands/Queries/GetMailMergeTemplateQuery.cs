using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models.Results.MailMerge;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetMailMergeTemplateQuery : SingleQuery<Guid, MailMergeTemplateModel>
    {
        private readonly ISynergyContext _synergyContext;
        private readonly ILogger<GetMailMergePropertyFieldsQuery> _logger;

        public GetMailMergeTemplateQuery(ISynergyContext synergyContext, ILogger<GetMailMergePropertyFieldsQuery> logger)
        {
            this._synergyContext = synergyContext ?? throw new ArgumentNullException(nameof(synergyContext));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<MailMergeTemplateModel> ExecuteAsync(Guid templateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this._synergyContext.GetQueryable<TemplateFile>().Select(x => new MailMergeTemplateModel
            {
                Id = x.Id,
                GroupingType = x.TemplateType.GroupingType,
                FileId = x.FileId,
            }).FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken).ConfigureAwait(false);
        }
    }
}
