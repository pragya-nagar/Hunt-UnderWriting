using AutoMapper;

namespace Synergy.Underwriting.Domain.Mappings.History
{
    public class PropertyAuditProfile : Profile
    {
        public PropertyAuditProfile()
        {
            this.CreateMap<DAL.Queries.Entities.Property, DAL.Queries.Entities.PropertyAudit>(MemberList.Source)
            .ForSourceMember(x => x.Attachments, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.Delinquencies, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.LeadName, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.StateName, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.CountyName, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.GeneralLandUseCodeName, exp => exp.DoNotValidate())
            .ForSourceMember(x => x.InternalLandUseCodeName, exp => exp.DoNotValidate())
            ;
        }
    }
}
