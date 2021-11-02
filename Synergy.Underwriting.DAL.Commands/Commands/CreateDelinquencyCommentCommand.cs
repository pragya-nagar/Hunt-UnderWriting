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
    public class CreateDelinquencyCommentCommand : ICreateDelinquencyCommentCommand
    {
        private IMapper _mapper;
        private ISynergyContext _context;

        public CreateDelinquencyCommentCommand(ISynergyContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void Dispatch(CreateDelinquencyCommentModel comment, Guid userId)
        {
            this.AddEntity(comment, userId);
            this._context.SaveChanges();
        }

        public Task<int> DispatchAsync(CreateDelinquencyCommentModel comment, Guid userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.AddEntity(comment, userId);
            return this._context.SaveChangesAsync(cancellationToken);
        }

        private void AddEntity(CreateDelinquencyCommentModel comment, Guid userId)
        {
            var entity = this._mapper.Map<DelinquencyComment>(comment).OnCreateAudit(userId);
            entity.AuthorId = userId;
            entity.CommentDate = entity.CreatedOn;

            this._context.DelinquencyComment.Add(entity);
        }
    }
}
