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
    public class CreateDataCutRuleCommand : ICreateDataCutRuleCommand
    {
        private IMapper _mapper;
        private ISynergyContext _context;

        public CreateDataCutRuleCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(IEnumerable<CreateDataCutRuleModel> entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(IEnumerable<CreateDataCutRuleModel> entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = _mapper.Map<List<DataCutRule>>(entity);

            data.ForEach(x =>
            {
                x.OnCreateAudit(userId);
                x.DataCutRuleItems.ToList().ForEach(a => a.OnCreateAudit(userId));
            });
            _context.DataCutRule.AddRange(data);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
