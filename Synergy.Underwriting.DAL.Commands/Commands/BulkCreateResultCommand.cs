using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BulkCreateResultCommand : IBulkCreateResultCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public BulkCreateResultCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(IEnumerable<CreateResultModel> model, Guid userId)
        {
            if (model.Any() == false)
            {
                return;
            }

            var entityList = this._mapper.Map<IEnumerable<Result>>(model).Select(x => x.OnCreateAudit(userId));
            this._context.Result.AddRange(entityList);

            this._context.SaveChanges();
        }

        public async Task<int> DispatchAsync(IEnumerable<CreateResultModel> model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (model.Any() == false)
            {
                return 0;
            }

            var entityList = this._mapper.Map<IEnumerable<Result>>(model).Select(x => x.OnCreateAudit(userId));
            await this._context.Result.AddRangeAsync(entityList, cancellationToken).ConfigureAwait(false);

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
