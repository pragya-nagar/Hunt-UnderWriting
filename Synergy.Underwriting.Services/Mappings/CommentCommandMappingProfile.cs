using AutoMapper;
using Synergy.Underwriting.DAL.Commands.Models;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Comment;

namespace Synergy.Underwriting.Services.Mappings
{
    public class CommentCommandMappingProfile : Profile
    {
        public CommentCommandMappingProfile()
        {
            this.CreateMap<DelinquencyCommentCreateCommand, CreateDelinquencyCommentModel>();
        }
    }
}
