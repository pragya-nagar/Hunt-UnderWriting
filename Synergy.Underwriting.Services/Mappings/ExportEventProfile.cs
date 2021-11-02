using System.Linq;
using AutoMapper;
using Synergy.Underwriting.DAL.Commands.Models.Results;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.Services.Mappings
{
    public class ExportEventProfile : Profile
    {
        public ExportEventProfile()
        {
            this.CreateMap<ExportMortgageModel, MortgageDumpModel>();

            this.CreateMap<ExportLevelDumpModel, LevelDumpModel>();

            this.CreateMap<ExportEventModel, EventDumpModel>(MemberList.Source)
                .ForSourceMember(x => x.Id, x => x.DoNotValidate());

            this.CreateMap<ExportPropertyModel, EventDumpModel>(MemberList.Source)
                .ForMember(x => x.DisplayStrategies, x => x.MapFrom(property =>
                    property.DisplayStrategies != null && property.DisplayStrategies.Any() == true
                        ? string.Join("; ", property.DisplayStrategies)
                        : string.Empty))
                .ForMember(x => x.Mortgage, x => x.MapFrom(property => new[] { property.Mortgage1, property.Mortgage2 }))
                .ForMember(x => x.InternalDelinquencyId, x => x.MapFrom(property => property.DeliquencyId.ToString()))
                .ForSourceMember(x => x.Mortgage1, x => x.DoNotValidate())
                .ForSourceMember(x => x.Mortgage2, x => x.DoNotValidate())
                .ForSourceMember(x => x.DisplayStrategies, x => x.DoNotValidate())
                .ForSourceMember(x => x.PropertyStateId, x => x.DoNotValidate())
                ;
        }
    }
}