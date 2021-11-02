using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class DataCutPropertyMapProfile : Profile
    {
        public DataCutPropertyMapProfile()
        {
            CreateMap<Delinquency, DataCutPropertyModel>()
                .ForMember(e => e.Id, t => t.MapFrom(src => src.Id))
                .ForMember(e => e.PropertyId, t => t.MapFrom(src => src.Property.Id))
                .ForMember(e => e.AccountName, t => t.MapFrom(src => src.Property.Lead.AccountName))
                .ForMember(e => e.PropertyAddress, t => t.MapFrom(src => src.Property.Address))
                .ForMember(e => e.PropertyZipCode, t => t.MapFrom(src => src.Property.ZipCode))
                .ForMember(e => e.LandUseCode, t => t.MapFrom(src => src.Property.LandUseCode))
                .ForMember(e => e.LTVPercent, t => t.MapFrom(d => d.Property.LTVPercent))
                .ForMember(e => e.RULTVPercent, t => t.MapFrom(d => d.Property.RULTVPercent))
                .ForMember(e => e.RUAmount, t => t.MapFrom(d => d.Property.RUAmount))
                .ForMember(e => e.StateId, t => t.MapFrom(src => src.Property.StateId))
                .ForMember(e => e.InternalLandUseCode, t => t.MapFrom(src => src.Property.InternalLandUseCode.Description))
                .ForMember(e => e.GeneralLandUseCode, t => t.MapFrom(src => src.Property.GeneralLandUseCode.Name))
                .ForMember(e => e.ImprovementValue, t => t.Ignore())
                .ForMember(e => e.AppraisedValue, t => t.Ignore())
                .ForMember(e => e.LandValue, t => t.Ignore())
                .ForMember(e => e.OpenLiens, t => t.MapFrom(d => d.PropertySupplementalEventData.OpenLiens))
                .ForMember(e => e.ClosedLiens, t => t.MapFrom(d => d.PropertySupplementalEventData.ClosedLiens))
                ;
        }
    }
}
