using AutoMapper;
using Synergy.DataAccess.Enum;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Commands.Attachment;

namespace Synergy.Underwriting.Services.Mappings
{
    public class PropertyCommandsMappingProfile : Profile
    {
        public PropertyCommandsMappingProfile()
        {
            this.CreateMap<PropertyUpdateCommand, DAL.Commands.Models.UpdatePropertyModel>()
                .ForMember(x => x.PropertyScoring, exp => exp.MapFrom(x => x.Scoring))
                .ForMember(x => x.DispStrategyIds, exp => exp.MapFrom(x => x.DisplayStrategyIds));

            this.CreateMap<PropertyAttachmentCreateCommand, DAL.Commands.Models.AttachFileToPropertyModel>()
                .ForMember(x => x.AttachmentType, exp => exp.Ignore())
                .ForMember(x => x.ContentType, exp => exp.Ignore());

            this.CreateMap<PropertyAttachmentDeleteCommand, DAL.Commands.Models.DeleteAttachmentModel>();
        }
    }
}
