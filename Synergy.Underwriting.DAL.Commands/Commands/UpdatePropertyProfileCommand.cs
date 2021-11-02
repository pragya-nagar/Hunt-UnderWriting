using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions.Commands;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Interfaces;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.Commands
{
    public class UpdatePropertyProfileCommand : IUpdatePropertyProfileCommand
    {
        private readonly IMapper _mapper;
        private readonly ISynergyContext _context;

        public UpdatePropertyProfileCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(UpdatePropertyProfileModel entity, Guid userId)
        {
            this.DispatchAsync(entity, userId).Wait();
        }

        public async Task<int> DispatchAsync(UpdatePropertyProfileModel entity, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var propertyProfileRules = _context.PropertyProfileRulePropertyProfile.Where(x => x.PropertyProfileId == entity.Id).ToList();
            var propertyProfileStates = _context.PropertyProfileState.Where(x => x.PropertyProfileId == entity.Id).ToList();

            if (propertyProfileRules.Any() == true)
            {
                _context.PropertyProfileRulePropertyProfile.RemoveRange(propertyProfileRules);
            }

            if (propertyProfileStates.Any() == true)
            {
                _context.PropertyProfileState.RemoveRange(propertyProfileStates);
            }

            PropertyProfile propertyProfile = await _context.PropertyProfile.SingleAsync(p => p.Id == entity.Id, cancellationToken).ConfigureAwait(false);
            _mapper.Map(entity, propertyProfile);
            propertyProfile.OnModifyAudit(userId);

            propertyProfile.PropertyProfileRulePropertyProfiles.ToList().ForEach(x => x.OnCreateAudit(userId));
            propertyProfile.PropertyProfileStates.ToList().ForEach(x => x.OnCreateAudit(userId));
            _context.PropertyProfile.Update(propertyProfile);
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
