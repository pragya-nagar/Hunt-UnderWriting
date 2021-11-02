using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateDelinquencyCommentModelMapProfile : Profile
    {
        public CreateDelinquencyCommentModelMapProfile()
        {
            CreateMap<CreateDelinquencyCommentModel, DelinquencyComment>()
                .IgnoreAuditMembers()
                .ForMember(c => c.Author, src => src.Ignore())
                .ForMember(c => c.CommentDate, src => src.Ignore())
                .ForMember(c => c.Delinquency, src => src.Ignore())
                .ForMember(c => c.AuthorId, src => src.Ignore())
                ;
        }
    }
}
