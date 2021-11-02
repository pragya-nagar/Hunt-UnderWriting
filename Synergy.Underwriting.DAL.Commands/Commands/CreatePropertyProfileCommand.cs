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
    public class CreatePropertyProfileCommand : ICreatePropertyProfileCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public CreatePropertyProfileCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(CreatePropertyProfileModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(CreatePropertyProfileModel entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = _mapper.Map<PropertyProfile>(entity).OnCreateAudit(userId);
            data.PropertyProfileRulePropertyProfiles.ToList().ForEach(x =>
            {
                x.Id = Guid.NewGuid();
                x.OnCreateAudit(userId);
            });

            data.PropertyProfileStates.ToList().ForEach(x =>
            {
                x.Id = Guid.NewGuid();
                x.OnCreateAudit(userId);
            });

            _context.PropertyProfile.Add(data);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}