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
    public class CreatePropertyProfileRuleCommand : ICreatePropertyProfileRuleCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreatePropertyProfileRuleCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(CreatePropertyProfileRuleModel createRuleModel, Guid userId)
        {
            this.DispatchAsync(createRuleModel, userId).Wait();
        }

        public async Task<int> DispatchAsync(CreatePropertyProfileRuleModel createRuleModel, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = _mapper.Map<PropertyProfileRule>(createRuleModel);

            data.OnCreateAudit(userId).PropertyProfileRuleItems.ToList().ForEach(a =>
                {
                    a.Id = Guid.NewGuid();
                    a.OnCreateAudit(userId);
                    a.PropertyProfileRuleId = data.Id;
                    a.PropertyProfileRuleItemValues.ToList().ForEach(v =>
                    {
                        v.Id = Guid.NewGuid();
                        v.OnCreateAudit(userId);
                        v.PropertyProfileRuleItemId = a.Id;
                    });
                });

            _context.PropertyProfileRule.AddRange(data);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}