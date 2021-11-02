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
    public class CreateOperationStatusCommand : ICreateOperationStatusCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreateOperationStatusCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(CreateOperationStatusModel model, Guid userId)
        {
            var entity = this._mapper.Map<OperationStatus>(model);

            if (entity.CreatedOn == DateTime.MinValue)
            {
                entity.OnCreateAudit(userId);
            }
            else
            {
                entity.ModifiedOn = entity.CreatedOn;
                entity.CreatedById = userId;
                entity.ModifiedById = userId;
            }

            this._context.OperationStatus.Add(entity);
            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(CreateOperationStatusModel model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = this._mapper.Map<OperationStatus>(model);

            if (entity.CreatedOn == DateTime.MinValue)
            {
                entity.OnCreateAudit(userId);
            }
            else
            {
                entity.ModifiedOn = entity.CreatedOn;
                entity.CreatedById = userId;
                entity.ModifiedById = userId;
            }

            await this._context.OperationStatus.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
