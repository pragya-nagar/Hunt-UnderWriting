using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class UpdateBidCommand : IUpdateBidCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public UpdateBidCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(UpdateBidModel model, Guid userId)
        {
            var entity = this._context.Bid.Single(x => x.Id == model.Id).OnModifyAudit(userId);
            this._mapper.Map(model, entity);

            this._context.Bid.Update(entity);
            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(UpdateBidModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = await _context.Bid.SingleAsync(x => x.Id == model.Id, cancellationToken).ConfigureAwait(false);
            entity.OnModifyAudit(userId);

            this._mapper.Map(model, entity);

            this._context.Bid.Update(entity);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
