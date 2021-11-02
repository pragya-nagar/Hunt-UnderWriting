using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateEventModelMapProfile : Profile
    {
        public CreateEventModelMapProfile()
        {
            CreateMap<CreateEventModel, Event>()
                .IncludeBase<UpdateEventModel, Event>()
                .ForMember(x => x.EventNumber, x => x.MapFrom(m => m.EventNumber))
                ;
        }
    }
}
