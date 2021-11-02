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
    public class BulkUpdateResultCommand : IBulkUpdateResultCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public BulkUpdateResultCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(IEnumerable<UpdateResultModel> model, Guid userId)
        {
            this.DispatchAsync(model, userId).GetAwaiter().GetResult();
        }

        public async Task<int> DispatchAsync(IEnumerable<UpdateResultModel> model, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ids = model.Select(x => x.Id);
            if (ids.Any() == false)
            {
                return 0;
            }

            foreach (var m in model)
            {
                var res = this._mapper.Map<Result>(m);

                res.OnModifyAudit(userId);

                this._context.Result.Update(res);
            }

            return await this._context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
