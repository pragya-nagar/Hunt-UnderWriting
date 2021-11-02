using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class UpdateResultModelMapProfile : Profile
    {
        public UpdateResultModelMapProfile()
        {
            this.CreateMap<UpdateResultModel, Result>()
                .IncludeBase<CreateResultModel, Result>()
                .ForMember(x => x.CreatedById, expression => expression.MapFrom(x => x.CreatedById))
                .ForMember(x => x.CreatedOn, expression => expression.MapFrom(x => x.CreatedOn));
        }
    }
}
