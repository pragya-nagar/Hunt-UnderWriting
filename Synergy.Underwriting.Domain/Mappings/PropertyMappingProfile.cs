using System.Linq;
using AutoMapper;
using Synergy.Underwriting.DAL.Queries.Original.Models;
using Synergy.Underwriting.Models.Address;
using Synergy.Underwriting.Models.Attachment;
using Synergy.Underwriting.Models.Commands;
using Synergy.Underwriting.Models.Property;

namespace Synergy.Underwriting.Domain.Mappings
{
    public class PropertyMappingProfile : Profile
    {
        public PropertyMappingProfile()
        {
            this.CreateMap<PropertyFilterArgs, PropertyFieldsFilterModel>();

            this.CreateMap<DAL.Queries.Original.Models.PropertyDecisionModel, DecisionModel>()
                .ForMember(x => x.Type, exp => exp.MapFrom(x => x.DecisionType))
                .ForMember(x => x.Level, exp => exp.MapFrom(x => x.EventDecisionLevel));

            this.CreateMap<DAL.Queries.Original.Models.EventDecisionLevelModel, DecisionLevelModel>();

            this.CreateMap<DAL.Queries.Original.Models.PropertyAssignmentModel, Underwriting.Models.Property.PropertyAssignmentModel>()
                .ForMember(x => x.TaxRatio, exp => exp.Ignore())
                .ForMember(x => x.DeletedOn, exp => exp.Ignore())
                .ForMember(x => x.CurrentDecision, exp => exp.MapFrom(item => item.Decisions.Where(x => x.DecisionType != null).OrderByDescending(x => x.EventDecisionLevel.Order).Select(x => x.DecisionType).FirstOrDefault()))
                .ForMember(x => x.AppraisedValue, exp => exp.MapFrom(x => x.PropertyValuations.OrderByDescending(y => y.AppraisedYear).Select(y => y.AppraisedValue).FirstOrDefault()))
                .ForMember(x => x.LandValue, exp => exp.MapFrom(x => x.PropertyValuations.OrderByDescending(y => y.AppraisedYear).Select(l => l.LandValue).FirstOrDefault()))
                .ForMember(x => x.ImprovementValue, exp => exp.MapFrom(x => x.PropertyValuations.OrderByDescending(y => y.AppraisedYear).Select(i => i.ImprovementValue).FirstOrDefault()))
                .ForMember(x => x.Ltv, exp => exp.MapFrom(x => x.LTV))
                .ForMember(x => x.RuLtv, exp => exp.MapFrom(x => x.RULTV))
                .ForMember(x => x.RuAmount, exp => exp.MapFrom(x => x.RUAmount))
                .ForMember(x => x.Address, exp => exp.MapFrom(x => new AddressModel { City = x.City, Zip = x.ZipCode, Address1 = x.Address }))
                .ForPath(x => x.Address.State, exp => exp.MapFrom(x => x.State))
                .ForMember(x => x.CadId, exp => exp.MapFrom(x => x.CADId))
                .ForMember(x => x.TaxId, exp => exp.MapFrom(x => x.TAXId))
                .ForMember(x => x.DisplayStrategy, exp => exp.MapFrom(x => x.PropertyDisplayStrategiesIds))
                .ForMember(x => x.Scoring, exp => exp.MapFrom(x => x.PropertyScoring))
                .ForMember(x => x.IsHomestead, exp => exp.MapFrom(x => x.Homestead))
                .ForMember(x => x.Supplemental, exp => exp.MapFrom(x => x.PropertySupplementalEventData))
                .ForMember(x => x.AmountDue, exp => exp.MapFrom(x => x.TotalAmountDue))
                ;

            this.CreateMap<DAL.Queries.Original.Models.PropertySupplementalEventDataModel, SupplementalModel>()
                .ForMember(x => x.MortgageList, exp => exp.MapFrom(x => x.MortgageData));
            this.CreateMap<DAL.Queries.Original.Models.PropertyAttachmentModel, Underwriting.Models.Property.PropertyAttachmentModel>();

            this.CreateMap<DAL.Queries.Original.Models.MortgageDataModel, MortgageModel>();
        }
    }
}
