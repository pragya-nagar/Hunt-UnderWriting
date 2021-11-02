using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Context;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Interfaces;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.Queries
{
    public class GetEventAttachmentQuery : BaseQuery<EventAttachment>, IGetEventAttachmentQuery
    {
        private IMapper _mapper;
        private DbSet<EventAttachment> _entity;

        public GetEventAttachmentQuery(ISynergyContext context, IMapper mapper)
        {
            _mapper = mapper;
            _entity = context.EventAttachment;
        }

        public IGetEventAttachmentQuery FindById(Guid id)
        {
            andAlsoPredicates.Add(u => u.Id == id);
            return this;
        }

        public EventAttachmentModel Exeсute()
        {
            IQueryable<EventAttachment> query = BuildQuery();
            var data = _mapper.Map<EventAttachmentModel>(query.SingleOrDefault());

            // TODO add real data retrieving here!
            data.Data = new byte[] { 2, 4, 6, 8, 10 };

            return data;
        }

        public async Task<EventAttachmentModel> ExeсuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryable<EventAttachment> query = BuildQuery();
            var data = _mapper.Map<EventAttachmentModel>(await query.SingleOrDefaultAsync().ConfigureAwait(false));

            // TODO add real data retrieving here!
            data.Data = new byte[] { 2, 4, 6, 8, 10 };

            return data;
        }

        private IQueryable<EventAttachment> BuildQuery()
        {
            IQueryable<EventAttachment> query = _entity.Include(x => x.CreatedBy).Where(GetPredicate());

            return query;
        }
    }
}