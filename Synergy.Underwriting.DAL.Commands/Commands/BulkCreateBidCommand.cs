using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class BulkCreateBidCommand : IBulkCreateBidCommand
    {
        private readonly IMapper _mapper;
        private readonly ILogger<BulkCreateBidCommand> _logger;
        private readonly ISynergyContext _context;

        public BulkCreateBidCommand(ISynergyContext context, IMapper mapper, ILogger<BulkCreateBidCommand> logger)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispatch(IEnumerable<CreateBidModel> model, Guid userId)
        {
            this.DispatchAsync(model, userId).Wait();
        }

        public async Task<int> DispatchAsync(IEnumerable<CreateBidModel> list, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entityList = list.Select(x => this._mapper.Map<Bid>(x).OnCreateAudit(userId)).ToList();

            this._logger.LogInformation("Found {count} bids to insert into db.", entityList.Count);

            this._context.Bid.AddRange(entityList);

            var count = await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            this._logger.LogInformation("Changes saved successfully. {count} records affected", count);

            return count;
        }
    }
}
