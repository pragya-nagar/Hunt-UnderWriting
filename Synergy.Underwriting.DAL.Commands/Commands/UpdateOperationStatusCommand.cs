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
    public class UpdateOperationStatusCommand : IUpdateOperationStatusCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public UpdateOperationStatusCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(UpdateOperationStatusModel model, Guid userId)
        {
            var entity = this._context.OperationStatus.Single(x => x.Id == model.Id).OnModifyAudit(userId);
            this._mapper.Map(model, entity);

            this._context.OperationStatus.Update(entity);
            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(UpdateOperationStatusModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = await this._context.OperationStatus.SingleAsync(x => x.Id == model.Id, cancellationToken).ConfigureAwait(false);
            entity.OnModifyAudit(userId);

            this._mapper.Map(model, entity);

            this._context.OperationStatus.Update(entity);
            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
