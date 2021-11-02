using AutoMapper;

namespace Synergy.Underwriting.Domain.Mappings.History
{
    public class DelinquencyAuditProfile : Profile
    {
        public DelinquencyAuditProfile()
        {
            this.CreateMap<DAL.Queries.Entities.Delinquency, DAL.Queries.Entities.DelinquencyAudit>(MemberList.Source)
            .ForSourceMember(x => x.Property, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.Event, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.Result, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.SupplementalData, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.Decisions, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.EventDataCutDecisions, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.PropertyProfileDelinquencies, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.DelinquencyPropertyDisplayStrategy, exp => exp.DoNotValidate())
            ;
        }
    }
}
