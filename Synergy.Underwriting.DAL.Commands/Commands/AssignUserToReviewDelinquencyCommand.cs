using System;
using System.Collections.Generic;
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
    public class AssignUserToReviewDelinquencyCommand : IAssignUserToReviewDelinquencyCommand
    {
        private IMapper _mapper;
        private ISynergyContext _context;

        public AssignUserToReviewDelinquencyCommand(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void Dispatch(List<AssignUserToReviewDelinquencyModel> entity, Guid userId)
        {
            var data = _mapper.Map<List<Decision>>(entity);

            data.ForEach(d => d.OnCreateAudit(userId));

            _context.Decision.AddRange(data);
            _context.SaveChanges();
        }

        public Task<int> DispatchAsync(List<AssignUserToReviewDelinquencyModel> entity, Guid userId, CancellationToken cancellationToken = default)
        {
            var data = _mapper.Map<List<Decision>>(entity);
            data.ForEach(d => d.OnCreateAudit(userId));

            _context.Decision.AddRangeAsync(data, cancellationToken);
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
