using System;
using AutoMapper;
using Synergy.DataAccess.Abstractions.Models;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class DelinquencyCommentMapProfile : Profile
    {
        public DelinquencyCommentMapProfile()
        {
            CreateMap<DelinquencyComment, DelinquencyCommentModel>()
                .ForMember(e => e.Author, t => t.MapFrom(src => new FastEntityModel<Guid> { Id = src.AuthorId, Name = $"{src.Author.FirstName} {src.Author.LastName}" }))
                ;
        }
    }
}
