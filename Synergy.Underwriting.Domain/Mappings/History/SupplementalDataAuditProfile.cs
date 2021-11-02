using AutoMapper;

namespace Synergy.Underwriting.Domain.Mappings.History
{
    public class SupplementalDataAuditProfile : Profile
    {
        public SupplementalDataAuditProfile()
        {
            this.CreateMap<DAL.Queries.Entities.SupplementalData, DAL.Queries.Entities.SupplementalDataAudit>(MemberList.Source)
            .ForSourceMember(x => x.Delinquency, exp => exp.DoNotValidate())
            ;
        }
    }
}
