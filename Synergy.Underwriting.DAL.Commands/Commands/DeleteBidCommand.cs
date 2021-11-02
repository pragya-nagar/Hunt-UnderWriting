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
    public class DeleteBidCommand : IDeleteBidCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public DeleteBidCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(DeleteBidModel model, Guid userId)
        {
            DispatchAsync(model, userId).GetAwaiter().GetResult();
        }

        public async Task<int> DispatchAsync(DeleteBidModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entities = await _context.Bid
                .Where(x => model.BidIds.Contains(x.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var entity in entities)
            {
                entity.OnDeleteAudit(userId);
                this._context.Bid.Update(entity);
            }

            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
