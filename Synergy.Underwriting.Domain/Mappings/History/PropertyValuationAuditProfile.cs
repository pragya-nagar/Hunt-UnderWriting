using AutoMapper;

namespace Synergy.Underwriting.Domain.Mappings.History
{
    public class PropertyValuationAuditProfile : Profile
    {
        public PropertyValuationAuditProfile()
        {
            this.CreateMap<DAL.Queries.Entities.PropertyValuation, DAL.Queries.Entities.PropertyValuationAudit>(MemberList.Source);
        }
    }
}
