using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands.Interfaces;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.Queries
{
    public class GetMailMergeEventFieldsQuery : SingleQuery<Guid, ExportEventModel>
    {
        private readonly ISynergyContext _synergyContext;
        private readonly IConfigurationProvider _configuration;

        public GetMailMergeEventFieldsQuery(ISynergyContext synergyContext, IConfigurationProvider configuration)
        {
            this._synergyContext = synergyContext ?? throw new ArgumentNullException(nameof(synergyContext));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<ExportEventModel> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await this._synergyContext.Event
                .Where(x => x.Id == eventId)
                .ProjectTo<ExportEventModel>(this._configuration)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
