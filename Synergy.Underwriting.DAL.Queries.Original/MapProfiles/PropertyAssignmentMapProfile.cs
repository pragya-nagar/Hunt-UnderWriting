using System.Collections.Generic;
using AutoMapper;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class PropertyAssignmentMapProfile : Profile
    {
        public PropertyAssignmentMapProfile()
        {
            CreateMap<Property, PropertyAssignmentModel>()
                .ForMember(e => e.PropertyId, t => t.MapFrom(d => d.Id))
                .ForMember(e => e.GeneralLandUseCode, t => t.MapFrom(src => src.GeneralLandUseCodeId))
                .ForMember(e => e.County, t => t.MapFrom(src => src.County))
                .ForMember(dest => dest.IsLatestPropertyData, opt => opt.Ignore())
                .ForMember(dest => dest.Decisions, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore())
                .ForMember(dest => dest.DelinquencyYear, opt => opt.Ignore())
                .ForMember(dest => dest.PropertySupplementalEventData, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyDisplayStrategiesIds, t => t.Ignore())
                .ForMember(dest => dest.PropertyScoring, t => t.Ignore())
                .ForMember(dest => dest.PropertyAttachments, t => t.Ignore())
                .ForMember(dest => dest.DataCutDecision, t => t.Ignore())
                .ForMember(dest => dest.TotalAmountDue, opt => opt.Ignore())
                .ForMember(e => e.LTV, t => t.Ignore())
                .ForMember(e => e.RULTV, t => t.Ignore())
                .ForMember(e => e.RUAmount, t => t.Ignore())
                ;

            CreateMap<Decision, PropertyDecisionModel>()
                .ForMember(dest => dest.DecisionType, opt => opt.MapFrom(src => src.DecisionTypeId))
                .ForMember(dest => dest.EventDecisionLevel, opt => opt.MapFrom(src => src.EventDecisionLevel))
                ;

            CreateMap<Decision, PropertyDecisionExportModel>()
                .ApplyAuditMembers()
                 .ForMember(dest => dest.DecisionType, opt => opt.MapFrom(src => src.DecisionTypeId))
                .ForMember(dest => dest.EventDecisionLevel, opt => opt.MapFrom(src => src.EventDecisionLevel))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                ;

            CreateMap<EventDecisionLevel, EventDecisionLevelModel>()
                ;

            CreateMap<PropertyValuation, PropertyValuationModel>()
                .ApplyAuditMembers()
                ;

            CreateMap<PropertySupplementalEventData, PropertySupplementalEventDataModel>()
                .ForMember(x => x.MortgageData, t => t.MapFrom(x => new List<MortgageDataModel>()
                {
                    new MortgageDataModel
                    {
                        MortgageDataNumber = 1,
                        MortgageLender = x.MortgageLender1,
                        MortgageLoanAmount = x.MortgageLoanAmount1,
                        MortgageMaturityDate = x.MortgageMaturityDate1,
                        MortgageOriginationDate = x.MortgageOriginationDate1,
                    },
                    new MortgageDataModel
                    {
                        MortgageDataNumber = 2,
                        MortgageLender = x.MortgageLender2,
                        MortgageLoanAmount = x.MortgageLoanAmount2,
                        MortgageMaturityDate = x.MortgageMaturityDate2,
                        MortgageOriginationDate = x.MortgageOriginationDate2,
                    },
                }));

            CreateMap<PropertyAttachment, PropertyAttachmentModel>()
                .ForMember(x => x.Id, t => t.MapFrom(x => x.Id))
                .ForMember(x => x.Name, t => t.MapFrom(x => x.FileName))
                .ForMember(x => x.UserId, t => t.MapFrom(x => x.CreatedById));
        }
    }
}
