using AutoMapper;
using Synergy.Underwriting.Models.Commands;

namespace Synergy.Underwriting.Services.Mappings
{
    public class ResultCommandsMappingProfile : Profile
    {
        public ResultCommandsMappingProfile()
        {
            this.CreateMap<Result, Underwriting.DAL.Commands.Models.CreateResultModel>();

            this.CreateMap<Result, Underwriting.DAL.Commands.Models.UpdateResultModel>();
        }
    }
}
