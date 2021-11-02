using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class CreateBidCommand : ICreateBidCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreateBidCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(CreateBidModel model, Guid userId)
        {
            var entity = this._mapper.Map<Bid>(model).OnCreateAudit(userId);

            this._context.Bid.Add(entity);
            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(CreateBidModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = this._mapper.Map<Bid>(model).OnCreateAudit(userId);

            await _context.Bid.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
