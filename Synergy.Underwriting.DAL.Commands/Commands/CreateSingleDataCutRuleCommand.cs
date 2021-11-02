using System;
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
    public class CreateSingleDataCutRuleCommand : ICreateSingleDataCutRuleCommand
    {
        private IMapper _mapper;
        private ISynergyContext _context;

        public CreateSingleDataCutRuleCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(CreateDataCutRuleModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(CreateDataCutRuleModel entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var data = _mapper.Map<DataCutRule>(entity);
            data.OnCreateAudit(userId);
            data.DataCutRuleItems.ToList().ForEach(a => a.OnCreateAudit(userId));

            _context.DataCutRule.Add(data);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
