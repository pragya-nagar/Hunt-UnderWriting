using System.Linq;
using AutoMapper;
using Synergy.Underwriting.DAL.Commands.Models.Results;
using Synergy.Underwriting.DAL.Commands.Models.Results.MailMerge;

namespace Synergy.Underwriting.Services.Mappings
{
    public class MergeSingleFieldsProfile : Profile
    {
        public MergeSingleFieldsProfile()
        {
            this.CreateMap<ExportEventModel, MergeSingleFields>(MemberList.Source)
                .ForMember(x => x.Event, x => x.MapFrom(property => property.EventNumber))
                .ForSourceMember(x => x.Id, x => x.DoNotValidate());

            this.CreateMap<MailMergePropertyModel, MergeSingleFields>(MemberList.Source)
                .ForMember(x => x.CampaignName, x => x.MapFrom(property => property.Campaign.CampaignName))
                .ForMember(x => x.CampaignType, x => x.MapFrom(property => property.Campaign.CampaignType))
                .ForMember(x => x.CampaignSubType, x => x.MapFrom(property => property.Campaign.CampaignSubType))
                .ForMember(x => x.CreatedDate, x => x.MapFrom(property => property.Campaign.CreatedDate))
                .ForMember(x => x.Description, x => x.MapFrom(property => property.Campaign.Description))
                .ForMember(x => x.TargetDate, x => x.MapFrom(property => property.Campaign.TargetDate))
                .ForMember(x => x.Note, x => x.MapFrom(property => property.Campaign.Note))
                .ForMember(x => x.AssignedUser, x => x.MapFrom(property => property.Campaign.AssignedUser))

                .ForMember(x => x.DisplayStrategies, x => x.MapFrom(property =>
                    property.DisplayStrategies != null && property.DisplayStrategies.Any() == true
                        ? string.Join("; ", property.DisplayStrategies)
                        : string.Empty))
                .ForMember(x => x.Mortgage1Loan, x => x.MapFrom(property => property.Mortgage1.Loan))
                .ForMember(x => x.Mortgage1Date, x => x.MapFrom(property => property.Mortgage1.MaturityDate))
                .ForMember(x => x.Mortgage2Loan, x => x.MapFrom(property => property.Mortgage2.Loan))
                .ForMember(x => x.Mortgage2Date, x => x.MapFrom(property => property.Mortgage2.MaturityDate))
                .ForMember(x => x.InternalDelinquencyId, x => x.MapFrom(property => property.DeliquencyId.ToString()))

                .ForSourceMember(x => x.ResultInterestRate, x => x.DoNotValidate())
                .ForSourceMember(x => x.FinalDecision, x => x.DoNotValidate())
                .ForSourceMember(x => x.FinalReason, x => x.DoNotValidate())
                .ForSourceMember(x => x.FinalReviewer, x => x.DoNotValidate())
                .ForSourceMember(x => x.EventId, x => x.DoNotValidate())
                .ForSourceMember(x => x.Campaign, x => x.DoNotValidate())

                .ForSourceMember(x => x.Mortgage1, x => x.DoNotValidate())
                .ForSourceMember(x => x.Mortgage2, x => x.DoNotValidate())
                .ForSourceMember(x => x.DisplayStrategies, x => x.DoNotValidate())
                .ForSourceMember(x => x.PropertyStateId, x => x.DoNotValidate())
                ;
        }
    }
}
