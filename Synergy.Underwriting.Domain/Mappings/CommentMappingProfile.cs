using AutoMapper;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.Domain.Mappings
{
    public class CommentMappingProfile : Profile
    {
        public CommentMappingProfile()
        {
            this.CreateMap<DAL.Queries.Original.Models.DelinquencyCommentModel, DelinquencyCommentModel>();
        }
    }
}